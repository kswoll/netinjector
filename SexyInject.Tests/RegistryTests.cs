﻿using System;
using NUnit.Framework;

namespace SexyInject.Tests
{
    [TestFixture]
    public class RegistryTests
    {
        [Test]
        public void ImplicitRegistration()
        {
            var registry = new Registry();
            var simpleClass = registry.Get<SimpleClass>();
            Assert.IsNotNull(simpleClass);
        }

        [Test]
        public void NonGenericGet()
        {
            var registry = new Registry();
            var simpleClass = registry.Get(typeof(SimpleClass));
            Assert.IsTrue(simpleClass is SimpleClass);
        }

        [Test]
        public void InterfaceRegistration()
        {
            var registry = new Registry();
            registry.Bind<ISimpleClass>().To<SimpleClass>();
            var simpleClass = registry.Get<ISimpleClass>();
            Assert.IsNotNull(simpleClass);
        }

        [Test]
        public void SimpleInjection()
        {
            var registry = new Registry();
            var injectionClass = registry.Get<InjectionClass>();
            Assert.IsNotNull(injectionClass.SimpleClass);
        }

        [Test]
        public void UnregisteredTypeThrowsWhenAllowImplicitRegistrationIsFalse()
        {
            var registry = new Registry(false);
            Assert.Throws<RegistryException>(() => registry.Get<SimpleClass>());
        }

        [Test]
        public void ClassWithoutConstructorThrows()
        {
            var registry = new Registry();
            Assert.Throws<ArgumentException>(() => registry.Get<ClassWithoutConstructor>());
        }

        [Test]
        public void PredicatedResolver()
        {
            var registry = new Registry();
            registry.Bind<ISomeInterface>().When(_ => false).To<SomeClass1>();
            registry.Bind<ISomeInterface>().When(_ => true).To<SomeClass2>();
            var impl = registry.Get<ISomeInterface>();
            Assert.IsTrue(impl is SomeClass2);
        }

        public interface ISimpleClass
        {
        }

        public class SimpleClass : ISimpleClass
        {
        }

        public class InjectionClass
        {
            public SimpleClass SimpleClass { get; }

            public InjectionClass(SimpleClass simpleClass)
            {
                SimpleClass = simpleClass;
            }
        }

        public class ClassWithoutConstructor
        {
            private ClassWithoutConstructor()
            {
            }
        }

        public interface ISomeInterface
        {
        }

        public class SomeClass1 : ISomeInterface
        {
        }

        public class SomeClass2 : ISomeInterface
        {
        }
    }
}