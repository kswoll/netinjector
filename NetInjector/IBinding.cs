﻿namespace NetInjector
{
    public interface IBinding
    {
        IResolver Resolver { get; }
        IScope Scope { get; }
    }
}