﻿using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SexyInject.Emit;

namespace SexyInject.Tests.Emit
{
    [TestFixture]
    public class ILNewExpressionTests
    {
        [Test]
        public void NoArgumentConstructor()
        {
            var registry = new Registry();
            var instance = registry.Construct(_ => new NoArgumentConstructorClass());
            Assert.IsNotNull(instance);
        }

        [Test]
        public void StructConstructor()
        {
            var registry = new Registry();
            registry.Bind<TestStruct>(x => x.To(_ => new TestStruct(5)));
            var instance = registry.Construct(_ => new OneArgumentConstructorWithDefaultValueStruct());
            Assert.AreEqual(5, instance.Other.Value);            
        }

        [Test]
        public void OneArgumentConstructorWithArgument()
        {
            var registry = new Registry();
            var other = new NoArgumentConstructorClass();
            var instance = registry.Construct(_ => new OneArgumentConstructorWithDefaultValue(other));
            Assert.AreEqual(other, instance.Other);
        }

        [Test]
        public void OneArgumentConstructorInjectArgument()
        {
            var registry = new Registry();
            var other = new NoArgumentConstructorClass();
            registry.Bind<NoArgumentConstructorClass>(x => x.To(_ => other));
            var instance = registry.Construct(_ => new OneArgumentConstructorWithDefaultValue());
            Assert.AreEqual(other, instance.Other);            
        }

        [Test]
        public void OneArgumentConstructorWithDefaultMinus1()
        {
            var registry = new Registry();
            registry.Bind<int>(x => x.To(_ => 5));
            var instance = registry.Construct(_ => new ClassOneArgumentConstructorWithDefaultMinus1());
            Assert.AreEqual(5, instance.Value);            
        }

        [Test]
        public void OneArgumentConstructorWithDefaultValueNullableInt()
        {
            var registry = new Registry();
            registry.Bind<int>(x => x.To(_ => (int)5));
            var instance = registry.Construct(_ => new ClassOneArgumentConstructorWithDefaultValueNullableInt());
            Assert.AreEqual(5, instance.Value);            
        }

        [Test]
        public void OneArgumentConstructorWithDefault0Short()
        {
            var registry = new Registry();
            registry.Bind<short>(x => x.To(_ => (short)5));
            var instance = registry.Construct(_ => new ClassOneArgumentConstructorWithDefault0Short());
            Assert.AreEqual(5, instance.Value);            
        }

        [Test]
        public void OneArgumentConstructorWithDefault1Long()
        {
            var registry = new Registry();
            registry.Bind<long>(x => x.To(_ => 5L));
            var instance = registry.Construct(_ => new ClassOneArgumentConstructorWithDefault1Long());
            Assert.AreEqual(5, instance.Value);            
        }

        [Test]
        public void OneArgumentConstructorWithDefault1Point3Float()
        {
            var registry = new Registry();
            registry.Bind<float>(x => x.To(_ => 15.3F));
            var instance = registry.Construct(_ => new ClassOneArgumentConstructorWithDefault1Point3Float());
            Assert.AreEqual(15.3F, instance.Value);            
        }

        [Test]
        public void OneArgumentConstructorWithDefault1000Point3Double()
        {
            var registry = new Registry();
            registry.Bind<double>(x => x.To(_ => 15.3D));
            var instance = registry.Construct(_ => new ClassOneArgumentConstructorWithDefault1000Point3Double());
            Assert.AreEqual(15.3D, instance.Value);            
        }

        [Test]
        public void OneArgumentConstructorWithDefault1000Point3Decimal()
        {
            var registry = new Registry();
            registry.Bind<decimal>(x => x.To(_ => 15.3M));
            var instance = registry.Construct(_ => new ClassOneArgumentConstructorWithDefault1000Point3Decimal());
            Assert.AreEqual(15.3M, instance.Value);            
        }

        [Test]
        public void OneArgumentConstructorWithDefault0()
        {
            var registry = new Registry();
            registry.Bind<int>(x => x.To(_ => 5));
            var instance = registry.Construct(_ => new ClassOneArgumentConstructorWithDefault0());
            Assert.AreEqual(5, instance.Value);            
        }

        [Test]
        public void OneArgumentConstructorWithDefault1()
        {
            var registry = new Registry();
            registry.Bind<int>(x => x.To(_ => 5));
            var instance = registry.Construct(_ => new ClassOneArgumentConstructorWithDefault1());
            Assert.AreEqual(5, instance.Value);            
        }

        [Test]
        public void OneArgumentConstructorWithDefault2()
        {
            var registry = new Registry();
            registry.Bind<int>(x => x.To(_ => 100));
            var instance = registry.Construct(_ => new ClassOneArgumentConstructorWithDefault2());
            Assert.AreEqual(100, instance.Value);            
        }

        [Test]
        public void OneArgumentConstructorWithDefault3()
        {
            var registry = new Registry();
            registry.Bind<int>(x => x.To(_ => 100));
            var instance = registry.Construct(_ => new ClassOneArgumentConstructorWithDefault3());
            Assert.AreEqual(100, instance.Value);            
        }

        [Test]
        public void OneArgumentConstructorWithDefault4()
        {
            var registry = new Registry();
            registry.Bind<int>(x => x.To(_ => 100));
            var instance = registry.Construct(_ => new ClassOneArgumentConstructorWithDefault4());
            Assert.AreEqual(100, instance.Value);            
        }

        [Test]
        public void OneArgumentConstructorWithDefault5()
        {
            var registry = new Registry();
            registry.Bind<int>(x => x.To(_ => 100));
            var instance = registry.Construct(_ => new ClassOneArgumentConstructorWithDefault5());
            Assert.AreEqual(100, instance.Value);            
        }

        [Test]
        public void OneArgumentConstructorWithDefault6()
        {
            var registry = new Registry();
            registry.Bind<int>(x => x.To(_ => 100));
            var instance = registry.Construct(_ => new ClassOneArgumentConstructorWithDefault6());
            Assert.AreEqual(100, instance.Value);            
        }

        [Test]
        public void OneArgumentConstructorWithDefault7()
        {
            var registry = new Registry();
            registry.Bind<int>(x => x.To(_ => 100));
            var instance = registry.Construct(_ => new ClassOneArgumentConstructorWithDefault7());
            Assert.AreEqual(100, instance.Value);            
        }

        [Test]
        public void OneArgumentConstructorWithDefault8()
        {
            var registry = new Registry();
            registry.Bind<int>(x => x.To(_ => 100));
            var instance = registry.Construct(_ => new ClassOneArgumentConstructorWithDefault8());
            Assert.AreEqual(100, instance.Value);            
        }

        [Test]
        public void OneArgumentConstructorWithDefault9()
        {
            var registry = new Registry();
            registry.Bind<int>(x => x.To(_ => 100));
            var instance = registry.Construct(_ => new ClassOneArgumentConstructorWithDefault9());
            Assert.AreEqual(100, instance.Value);            
        }

        [Test]
        public void OneArgumentConstructorWithDefault1000()
        {
            var registry = new Registry();
            registry.Bind<int>(x => x.To(_ => 100));
            var instance = registry.Construct(_ => new ClassOneArgumentConstructorWithDefault1000());
            Assert.AreEqual(100, instance.Value);            
        }

        [Test]
        public void AsyncConstructorsThrows()
        {
            var registry = new Registry();
#pragma warning disable 1998
            Assert.Throws<InvalidFactoryException>(() => registry.Construct(async _ => new ClassOneArgumentConstructorWithDefault1()));
#pragma warning restore 1998
        }

        [Test]
        public void CallLambda()
        {
            Func<int> getValue = () => 42;
            var registry = new Registry();
            var instance = registry.Construct(_ => new ClassOneArgumentConstructorWithDefault1(getValue()));
            Assert.AreEqual(42, instance.Value);
        }

        class NoArgumentConstructorClass
        {
        }

        class OneArgumentConstructorWithDefaultValueStruct
        {
            public TestStruct Other { get; }

            public OneArgumentConstructorWithDefaultValueStruct(TestStruct other = default(TestStruct))
            {
                Other = other;
            }
        }

        class OneArgumentConstructorWithDefaultValue
        {
            public NoArgumentConstructorClass Other { get; }

            public OneArgumentConstructorWithDefaultValue(NoArgumentConstructorClass other = null)
            {
                Other = other;
            }
        }

        class ClassOneArgumentConstructorWithDefaultValueNullableInt
        {
            public int? Value { get; }

            public ClassOneArgumentConstructorWithDefaultValueNullableInt(int? value = null)
            {
                Value = value;
            }
        }

        class ClassOneArgumentConstructorWithDefault0Short
        {
            public short Value { get; }

            public ClassOneArgumentConstructorWithDefault0Short(short value = 0)
            {
                Value = value;
            }
        }

        class ClassOneArgumentConstructorWithDefault1Long
        {
            public long Value { get; }

            public ClassOneArgumentConstructorWithDefault1Long(long value = 1)
            {
                Value = value;
            }
        }

        class ClassOneArgumentConstructorWithDefault1Point3Float
        {
            public float Value { get; }

            public ClassOneArgumentConstructorWithDefault1Point3Float(float value = 1.2F + .1F)
            {
                Value = value;
            }
        }

        class ClassOneArgumentConstructorWithDefault1000Point3Double
        {
            public double Value { get; }

            public ClassOneArgumentConstructorWithDefault1000Point3Double(double value = 1000.2F + .1F)
            {
                Value = value;
            }
        }

        class ClassOneArgumentConstructorWithDefault1000Point3Decimal
        {
            public decimal Value { get; }

            public ClassOneArgumentConstructorWithDefault1000Point3Decimal(decimal value = 1000.2M + .1M)
            {
                Value = value;
            }
        }

        class ClassOneArgumentConstructorWithDefault0
        {
            public int Value { get; }

            public ClassOneArgumentConstructorWithDefault0(int value = 0)
            {
                Value = value;
            }
        }

        class ClassOneArgumentConstructorWithDefault1
        {
            public int Value { get; }

            public ClassOneArgumentConstructorWithDefault1(int value = 1)
            {
                Value = value;
            }
        }

        class ClassOneArgumentConstructorWithDefaultMinus1
        {
            public int Value { get; }

            public ClassOneArgumentConstructorWithDefaultMinus1(int value = -1)
            {
                Value = value;
            }
        }

        class ClassOneArgumentConstructorWithDefault2
        {
            public int Value { get; }

            public ClassOneArgumentConstructorWithDefault2(int value = 2)
            {
                Value = value;
            }
        }

        class ClassOneArgumentConstructorWithDefault3
        {
            public int Value { get; }

            public ClassOneArgumentConstructorWithDefault3(int value = 3)
            {
                Value = value;
            }
        }

        class ClassOneArgumentConstructorWithDefault4
        {
            public int Value { get; }

            public ClassOneArgumentConstructorWithDefault4(int value = 4)
            {
                Value = value;
            }
        }

        class ClassOneArgumentConstructorWithDefault5
        {
            public int Value { get; }

            public ClassOneArgumentConstructorWithDefault5(int value = 5)
            {
                Value = value;
            }
        }

        class ClassOneArgumentConstructorWithDefault6
        {
            public int Value { get; }

            public ClassOneArgumentConstructorWithDefault6(int value = 6)
            {
                Value = value;
            }
        }

        class ClassOneArgumentConstructorWithDefault7
        {
            public int Value { get; }

            public ClassOneArgumentConstructorWithDefault7(int value = 7)
            {
                Value = value;
            }
        }

        class ClassOneArgumentConstructorWithDefault8
        {
            public int Value { get; }

            public ClassOneArgumentConstructorWithDefault8(int value = 8)
            {
                Value = value;
            }
        }

        class ClassOneArgumentConstructorWithDefault9
        {
            public int Value { get; }

            public ClassOneArgumentConstructorWithDefault9(int value = 9)
            {
                Value = value;
            }
        }

        class ClassOneArgumentConstructorWithDefault1000
        {
            public int Value { get; }

            public ClassOneArgumentConstructorWithDefault1000(int value = 1000)
            {
                Value = value;
            }
        }

        struct TestStruct
        {
            public int Value { get; set; }

            public TestStruct(int value)
            {
                Value = value;
            }
        }
    }
}