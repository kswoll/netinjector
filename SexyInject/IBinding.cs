﻿namespace SexyInject
{
    public interface IBinding
    {
        IResolver Resolver { get; }
        IScope Scope { get; }
    }
}