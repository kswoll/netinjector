# SexyInject

SexyInject is a dependency injection framework.  It is designed to be flexible such that you can create very general bindings that satisfy a rich set of types.  For example, many DI frameworks have built-in support for injecting `Func<T>` into your types such that it ends up being a factory function for `T`.  While SexyInject also supports this natively, it is implemented as a simple extension method off of `Registry` named `RegisterFactoryPattern`, which demonstrates that if the facility weren't already included, it would have been trivial for you to add that functionality yourself.  Later on, we'll take a look at that extension method and break it down piece by piece.

## Registry

The main class you interact with is an instance of `Registry`.   It is here where you specify how requests for some type `T` should be handled.  You might, for example, want to register an interface to resolve to some particular implementation. Or, you might want all requests for a type to resolve into the same instance (a singleton pattern).  Or all requests for an instantiatable type (i.e. non-abstract, etc.) to result in an instance of that type.  

That last example might seem curious — most DI frameworks support that without your having to do any work.  However, with this framework, a vanilla instance of `Registry` will never resolve any requests for any type.  It will simply throw an exception.  Since enabling various features of this sort are trivial one-liners, SexyInject adheres to an opt-in model where all the facilities you desire are explicitly enabled by you.  As an example, to enable the behavior where instantiatable types are automatically created when requested, you simply need this code:

```
var registry = new Registry();
registry.RegisterImplicitPattern();
```

Similarly, registering `Func<T>` factories only requires:

```
var registry = new Registry();
registry.RegisterFactoryPattern();
```

### Minimum Registration

But starting with a vanilla instance, what is the simplest possible example of binding something yourself?  Consider a class `Foo`:

```
public class Foo 
{
}
```

And requesting an instance of it:

```
var foo = registry.Get<Foo>();
```

Without any bindings set up, you'd simply get an exception indicating that no binding has been registered for type `Foo`. To create a binding so that a new instance is returned upon request, you'd add the line:

```
registry.Bind<Foo>();
```

Without doing anything else, this will just use the standard constructor and instantiate it.  This is the simplest possible binding, but not terribly interesting.  Furthermore, if you've called `RegisterImplicitPattern` then this happens for you automatically. 

### Associating an Interface with an Implementation

Let's move on to the canonical example of associating an implementation of an interface with that interface.  Let's assume you have the interface `IFoo`:

```
public interface IFoo
{
}
```

To make it so that requests for instances of `IFoo` return an instance of the class `Foo`, you'd add the following binding:

```
registry.Bind<IFoo>().To<Foo>();
```

Now, when you invoke `registry.Get<IFoo>()` you'll be returned an instance of `Foo`.

### Resolving Using Your Own Method or Lambda

The default constructor binding is often sufficient, but you can also provide your own logic to resolve a type.  As a trivial example, consider this binding that simply instantiates `Foo`:

```
registry.Bind<Foo>().To(targetType => new SimpleClass());
```

This overload of `To` allows you to supply a delegate taking one parameter (the “type being requested” — in this case `Foo` but that's not always the case) and another overload that allows you to supply two parameters, the `ResolveContext`, and the `targetType`.  More on that later.

### Constraining Bindings

Consider the following types:

```
public class StringClass
{
    public string StringProperty { get; }

    public StringClass(string stringProperty)
    {
        StringProperty = stringProperty;
    }
}

public class Consumer1
{
    public StringClass StringClass { get; }

    public Consumer1(StringClass stringClass)
    {
        StringClass = stringClass;
    }
}

public class Consumer2
{
    public StringClass StringClass { get; }

    public Consumer2(StringClass stringClass)
    {
        StringClass = stringClass;
    }
}
```

Now suppose we wanted a rule that makes it so when a request for `Consumer1` is made, an instance of `StringClass` is provided with the `StringProperty` set to “value1” and when a request for `Consumer2` is made, an instance of `StringClass` is provided with the `StringProperty` set to “value2”.  The binding for that could look like:

```
registry
    .Bind<StringClass>()
    .To(_ => new StringClass("value1"))
    .When((context, targetType) => context.CallerType == typeof(Consumer1));
registry
    .Bind<StringClass>()
    .To(_ => new StringClass("value2"))
    .When((context, targetType) => context.CallerType == typeof(Consumer2));
```

This is obviously a contrived example, and was mostly intended to demonstrate the `When` operator used for constraining a binding based on an arbitrary predicate you provide.  There are other ways to accomplish this sort of task, and the pattern you use will depend on your circumstances and taste.  Let's consider next an alternative solution.

### Explicit Injection

You can also inject dependencies explicitly using the `InjectArgument` operator.  This allows you to provide an instance of an object and make it available as a dependency to inject into the bound type.

```
registry
    .Bind<Consumer1>().To()
    .InjectArgument(_ => new StringClass("value1"));
registry
    .Bind<Consumer2>().To()
    .InjectArgument(_ => new StringClass("value2"));
```

As you can see, we're saying for `Consumer1`, provide an instance of `StringClass` to its constructor with its property set to “value1”.  In contrast, when obtaining an instance of `Consumer2` it will have been provided an instance with its property set to “value2”.

### Generalizing Registration

Sometimes you'll want to provide binding logic that is general enough that it is suitable for a variety of types.  In fact, one of the most general rules of all is the one mentioned earlier, `RegisterImplicitPattern`.  Its implementation is straightforward:

```
Func<Type, bool> isInstantiatable = type => !type.IsAbstract && !type.IsInterface && !type.IsGenericTypeDefinition;
registry
    .Bind<object>()
    .To((context, type) => context.Constructor(type))
    .When((context, targetType) => isInstantiatable(targetType));
```

To break this down:

* We are registering a binding for `object` which will catch all types as a fallback assuming a binding for a more specific type is not found.
* We are using the lambda pattern to provide an instance by using the `Constructor` function on the context.  This is subtly different from a separate method, `Construct`, which will be explained in detail later.
* We are constraining the binding to only those types which we could possibly instantiate — i.e. not abstract, an interface, etc.

The basic idea here is that if a type is requested for which no binding has been provided for that type itself, the type's hierarchy will be traversed.  To put it another way, each type in the following sequence will be traversed in order until a binding has been found for that type:

1. The requested type itself
2. If the requested type is a generic type, then its generic type definition
3. Each implemented interface of that type
4. The base type of that type, followed by its base type, all the way to `object`

In sum, you can see how given the right circumstances, you can create a binding that will have broad applicability.

### Caching

When creating generalized bindings as described in the last section, you occasionally need to perform some expensive operations for each type you are capable of resolving.  In such a situation, it often becomes desirable to cache things — in this case caching once per type.  To illustrate this concept, we'll examine the implementation of `RegisterFactoryPattern`.

```
registry
    .Bind(typeof(Func<>))
    .To((context, targetType) =>
        Expression.Lambda(
            targetType, 
            registry.GetExpression(targetType.GetGenericArguments()[0])).Compile())
    .Cache((context, targetType) => targetType);
```

Remember, the point of the factory pattern is to be able to inject an instance of `Func<T>` that when invoked will return an instance that has been resolved through your registry (generally speaking, this will return a new instance, but there's nothing stopping you from returning a singleton, or an instance derived from any other heuristic).

Now, before we completely break down this code, let's think about what that means in practice.  If you inject a `Func<StringClass>`, then there's no way to generically provide such a func without delving into some sort of dynamic code generation.  The simplest way to generate such code is by using expression trees in conjunction with `LambdaExpression`'s awesome `Compile` method.  Thus, by using expression trees we can rather trivially generate the `Func<T>`.  With that, let's now get into describing how `RegisterFactoryPattern` works.

1. We bind to `Func<>` so that when any request is made for *any* construction of it, this binding will be used.
2. We use the lambda overload of `.To` to provide a dynamically generated implementation of `Func<T>` that obtains an instance of `T` through the registry.  (`registry.GetExpression` is a helper method that returns an `Expression` representing an invocation to `Registry.Get`)
3. Finally, the `Cache` operator.  It's kind of like the LINQ operator `.GroupBy` in that you provide a lambda that returns a common “key” such that when the same key is returned the same instance will be returned.  So this is useful here because compiling an expression tree — while not *that* expensive — is still not something you want to do a million times for a million requests for `Func<T>` when a single instance will suffice.  This way, whenever an instance of `Func<T>` is requested, the same dynamically generated expression tree will be used instead of being regenerated each time.

## Resolving

As you've seen already, there's not much to it when it comes to requesting instances of a type.  You can use the framework as a service locator, as the examples above have shown.  Obviously you can also use it as a conventional DI framework and have your dependencies injected into your class via a constructor.  So say you have a new class, `Bar`, that looks like this:

```
public class Bar
{
    public Foo Foo { get; }

    public Bar(Foo foo)
    {
        Foo = foo;
    }
}
```

When you request an instance of `Bar`, this class would be instantiated and passed an instance of `Foo`.  The instance of `Foo` will be obtained from the registry and resolved as normal.  That may mean it's a fresh instance or that type or an instance obtained through some other heuristic you've supplied via registration.

One subtle detail is that when getting an instance from the registry, only one instance of a particular type will be resolved. To make this more clear, consider another type:

```
public class FooBar
{
    public Foo Foo { get; }
    public Bar Bar { get; }

    public FooBar(Foo foo, Bar bar)
    {
        Foo = foo;
        Bar = bar;
    }
}
```

Now to provide you with an instance of `FooBar`, an instance of `Foo` must be provided to `FooBar` and furthermore, an instance of `Foo` must be provided to `Bar`.  By default, only one instance of `Foo` will be resolved and that instance will be provided to the constructors of both `Bar` and `FooBar`.  You can opt out of this behavior when registering by supplying a cache policy:

```
registry.Bind<Foo>(CachePolicy.Never);
```

Now instances will never be (transiently) cached and you will get a separate instance for each class.  Keep in mind that this caching only applies to a single invocation to `Register.Get`.  Subsequent invocations will get their own cache and the cache will not live longer that that.


```
public static void RegisterFactoryPattern(this Registry registry)
{
    registry
        .Bind(typeof(Func<>))
        .To((context, targetType) => Expression.Lambda(targetType, registry.GetExpression(targetType.GetGenericArguments()[0])).Compile())
        .Cache((context, targetType) => targetType);
}
```

## Registration

Simple:

```
registry.Bind<IFoo>().To<Foo>();
```

Singleton:

```
registry.Bind<IFoo>().To(new Foo());
```

Lazy singleton:

```
registry.Bind<IFoo>().To(new Lazy<Foo>(() => new Foo()));
```

Predicate:

```
registry.Bind<IFoo>().To<BarFoo>().When(x => x.Container is Bar);
```

Any type not explicitly bound will still be resolved assuming it's not an abstract type, interface, or value type.  To disable this functionality and require explicit registration for all types:

```
registry.AllowImplicitRegistration = false;
```



## Resolving

Service locator style:

```
registry.Get<IFoo>();
```

Dependency injection style:

```
public class Bar
{
    public Bar(IFoo foo)
    {
        ...
    }
}

registry.Get<Bar>();    // The parameter "foo" will be resolved using dependency injection
```

Lazy:

```
var lazyFoo = registry.Get<Lazy<IFoo>>();
var foo = lazyFoo.Value;    // Will only instantiate or otherwise resolve "foo" at this point
```

Factory:

```
var fooFactory = registry.Get<Func<IFoo>>();
```

## 


## Caching

By default, each time you resolve an instance, dependency injection will only look up a dependency on a given type one time.  For example:

```
public class Bar
{
    public Bar(Foo foo, FooBar fooBar) {}
}

public class Foo 
{
    public Foo(FooBar fooBar) {}
}

registry.Resolve<Bar>();
```

In this scenario, both `Bar` and `Foo` have a dependency on `FooBar`, and both constructors will be passed the same instance of `FooBar` vs. creating a separate instance for each constructor.  Importantly, a subsequent call to `registry.Resolve<Bar>()` will start the process over and not re-use any existing instances (unless you are caching things during registration as in the case of singletons).

If you would like to avoid this behavior, then you must explicitly opt-out during registration:

```
registry.Bind<FooBar>(CachePolicy.Never);
```

The default behavior is equivalent to:

```
registry.Bind<FooBar>(CachePolicy.Transient);
```











