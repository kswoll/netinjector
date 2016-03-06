﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace SexyInject
{
    public class Binder 
    {
        public Registry Registry { get; }
        public Type Type { get; }

        private readonly object locker = new object();
        private readonly ConcurrentQueue<IResolver> resolvers = new ConcurrentQueue<IResolver>();
        private ConstructorResolver defaultResolver;
        private int defaultResolverCreated;

        public Binder(Registry registry, Type type)
        {
            Registry = registry;
            Type = type;
        }

        public void AddResolver(IResolver resolver)
        {
            resolvers.Enqueue(resolver);
        }

        public IEnumerable<IResolver> Resolvers => resolvers;

        public object Resolve(ResolverContext context)
        {
            bool isResolved;
            foreach (var resolver in resolvers)
            {
                var result = resolver.Resolve(context, out isResolved);
                if (isResolved)
                    return result;
            }
            if (Interlocked.CompareExchange(ref defaultResolverCreated, 0, 1) != 2)
            {
                lock (locker)
                {
                    defaultResolver = new ConstructorResolver(Type);
                    Interlocked.Exchange(ref defaultResolverCreated, 2);
                }
            }
            return defaultResolver.Resolve(context, out isResolved);
        }

        /// <summary>
        /// Binds requests for T to an instance of TTarget.
        /// </summary>
        /// <typeparam name="TTarget">The subclass of T (or T itself) to instantiate when an instance of T is requested.</typeparam>
        /// <param name="constructorSelector">A callback to select the constructor on TTarget to use when instantiating TTarget.  Defaults to null which 
        /// results in the selection of the first constructor with the most number of parameters.</param>
        public void To<TTarget>(Func<ConstructorInfo[], ConstructorInfo> constructorSelector = null)
        {
            AddResolver(new ConstructorResolver(typeof(TTarget)));
        }

        /// <summary>
        /// Binds requests for T to the result of a lambda function.
        /// </summary>
        /// <typeparam name="TTarget">The subclass of T (or T itself) that is returned when an instance of T is requested.</typeparam>
        /// <param name="resolver">The lambda function that returns the instance of the reuqested type.</param>
        public void To<TTarget>(Func<ResolverContext, TTarget> resolver)
        {
            AddResolver(new LambdaResolver(x => resolver(x)));
        }
    }

    public class Binder<T> : Binder
    {
        public Binder(Registry registry) : base(registry, typeof(T))
        {
        }

        public WhenContext<T> When(Func<ResolverContext, bool> predicate) => new WhenContext<T>(this, predicate);

        /// <summary>
        /// Binds requests for T to an instance of TTarget.
        /// </summary>
        /// <typeparam name="TTarget">The subclass of T (or T itself) to instantiate when an instance of T is requested.</typeparam>
        /// <param name="constructorSelector">A callback to select the constructor on TTarget to use when instantiating TTarget.  Defaults to null which 
        /// results in the selection of the first constructor with the most number of parameters.</param>
        public new void To<TTarget>(Func<ConstructorInfo[], ConstructorInfo> constructorSelector = null)
            where TTarget : class, T
        {
            AddResolver(new ConstructorResolver(typeof(TTarget)));
        }

        /// <summary>
        /// Binds requests for T to the result of a lambda function.
        /// </summary>
        /// <typeparam name="TTarget">The subclass of T (or T itself) that is returned when an instance of T is requested.</typeparam>
        /// <param name="resolver">The lambda function that returns the instance of the reuqested type.</param>
        public new void To<TTarget>(Func<ResolverContext, TTarget> resolver)
            where TTarget : class, T
        {
            AddResolver(new LambdaResolver(resolver));
        }        
    }
}