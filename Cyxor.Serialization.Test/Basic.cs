using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Cyxor.Extensions;
//using BenchmarkDotNet.Attributes;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Cyxor.Serialization.Test
{
    class Product
    {
        public string? Name { get; set; }
        public float Price { get; set; }
    }

    //class A
    //{
    //    private void A1() { }
    //    internal void A2() { }
    //    protected void A3() { }
    //    public void A4() { }
    //    static void A5() { }
    //    public static void A6() { }
    //}

    //class B : A
    //{
    //    private void B1() { }
    //    internal void B2() { }
    //    protected void B3() { }
    //    public void B4() { }
    //    static void B5() { }
    //    public static void B6() { }
    //}



    [TestClass]
    public class BasicTest
    {
        //[Benchmark]
        //public void Benchmarkxc()
        //{
        //    Thread.Sleep(5000);
        //}

        [TestMethod]
        public void Initialization()
        {
            //var ax = typeof(B).GetMethodsInfo();


            using var serialStream = new Serializer();
            var schema = Serializer.GenerateSerializationSchema();
            Assert.IsTrue(schema.Length > 0);

            //var genericTypes = SerialStream.SupportedTypes.Where(p => p.IsGenericType).Where(p => !p.GenericTypeArguments.First().IsGenericParameter);

            //var autoTypes = SerialStream.SupportedTypes.Where(p => !p.IsGenericType);

            //var union = genericTypes.Concat(autoTypes);

            //foreach (var type in autoTypes)
            //    if (!type.IsGenericTypeDefinition)
            //    {
            //        var obj = Activator.CreateInstance(type);
            //        serialStream.Serialize(obj);
            //    }
        }

        [TestMethod]
        public void SimpleObject()
        {
            var product = new Product { Name = "PC", Price = 450.00f };

            using var ss = new Serializer();
            ss.Serialize(product);

            ss.Position = 0;
            var newProduct = ss.DeserializeObject<Product>();

            Assert.AreEqual(product.Name, newProduct.Name);
            Assert.AreEqual(product.Price, newProduct.Price);
        }

        [TestMethod]
        public unsafe void SpanTest()
        {
            //void* n = null;
            //var span = new Span<byte>(n, 0);

            //var x = span.Length;
            //var y = span.IsEmpty;
            //var z = span.ToArray();
            //var w = MemoryMarshal.GetReference(span);

            //Unsafe.

            //byte b = MemoryMarshal.AsRef<byte>(span);

            Assert.IsTrue(true);
        }
    }
}
