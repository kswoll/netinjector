﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SexyInject
{
    public class Registry
    {
        private static readonly MethodInfo getMethod = typeof(Registry).GetMethods().Single(x => x.Name == nameof(Get) && x.GetParameters().Length == 2);
        private readonly ConcurrentDictionary<Type, Binder> binders = new ConcurrentDictionary<Type, Binder>();
        private readonly ConcurrentDictionary<Type, Func<ResolveContext, object>> factoryCache = new ConcurrentDictionary<Type, Func<ResolveContext, object>>();

        public Binder<T> Bind<T>(CachePolicy cachePolicy = CachePolicy.Transient)
        {
            return (Binder<T>)binders.GetOrAdd(typeof(T), x => new Binder<T>(this, cachePolicy));
        }

        public Binder Bind(Type type, CachePolicy cachePolicy = CachePolicy.Transient)
        {
            return binders.GetOrAdd(type, x => new Binder(this, type, cachePolicy));
        }

        public T Get<T>(params object[] arguments)
        {
            return (T)Get(typeof(T), CreateResolverContext(arguments));
        }

        public object Get(Type type, params object[] arguments)
        {
            return Get(type, CreateResolverContext(arguments));
        }

        public Expression GetExpression(Type type)
        {
            return Expression.Convert(Expression.Call(Expression.Constant(this), getMethod, Expression.Constant(type), Expression.Constant(new object[0])), type);
        }

        public T Construct<T>(Func<ConstructorInfo[], ConstructorInfo> constructorSelector = null)
        {
            return (T)Construct(typeof(T), constructorSelector);
        }

        public object Construct(Type type, params object[] arguments)
        {
            return Construct(CreateResolverContext(arguments), type, null);
        }

        public object Construct(Type type, Func<ConstructorInfo[], ConstructorInfo> constructorSelector, params object[] arguments)
        {
            return Construct(CreateResolverContext(arguments), type, constructorSelector);
        }

        private object Construct(ResolveContext context, Type type, Func<ConstructorInfo[], ConstructorInfo> constructorSelector)
        {
            return factoryCache.GetOrAdd(type, x => FactoryGenerator(x, constructorSelector))(context);
        }

        private ResolveContext CreateResolverContext(object[] arguments)
        {
            ResolveContext context = null;
            context = new ResolveContext(this, x => Get(x, context), Construct, arguments);
            return context;
        }

        private object Get(Type type, ResolveContext context)
        {
            Binder binder = null;
            foreach (var current in EnumerateBaseTypes(type))
            {
                if (binders.TryGetValue(current, out binder))
                    break;
            }
            if (binder == null)
                throw new RegistryException($"The type {type.FullName} has not been registered and AllowImplicitRegistration is disabled.");

            return binder.Resolve(context, type);
        }

        private Func<ResolveContext, object> FactoryGenerator(Type type, Func<ConstructorInfo[], ConstructorInfo> constructorSelector)
        {
            constructorSelector = constructorSelector ?? (constructors => constructors.OrderByDescending(x => x.GetParameters().Length).FirstOrDefault());

            var constructor = constructorSelector(type.GetConstructors());
            if (constructor == null)
                throw new ArgumentException($"Type {type.FullName} must have at least one public constructor", nameof(type));

            var parameters = constructor.GetParameters();
            var contextParameter = Expression.Parameter(typeof(ResolveContext), "context");

            // context.TryResolve(arg0Type), context.TryResolve(arg1Type)...
            var arguments = parameters.Select(x => Expression.Convert(ResolveContext.ResolveExpression(contextParameter, x.ParameterType), x.ParameterType)).ToArray();

            // new T(arguments)
            var body = Expression.New(constructor, arguments);

            // context => body
            var lambda = Expression.Lambda<Func<ResolveContext, object>>(body, contextParameter);

            // Compile it into a delegate we can actually invoke
            return lambda.Compile();
        }

        private IEnumerable<Type> EnumerateBaseTypes(Type type)
        {
            var current = type;
            while (current != null)
            {
                yield return current;

                if (current.IsGenericType)
                    yield return current.GetGenericTypeDefinition();

                current = current.BaseType;
            }
            foreach (var @interface in type.GetInterfaces())
            {
                yield return @interface;
                if (@interface.IsGenericType)
                    yield return @interface.GetGenericTypeDefinition();
            }
        }
    }
}