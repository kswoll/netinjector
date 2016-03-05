﻿namespace SexyInject
{
    public interface IBinder
    {
        Registry Registry { get; }
        object Resolve(ResolverContext context);
        void AddResolver(IResolver resolver);
    }
}