using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Cyxor.Extensions;

namespace Cyxor.Serialization.Test
{
    class Product
    {
        public string? Name { get; set; }
        public float Price { get; set; }
    }

    class A
    {
        private void A1() { }
        internal void A2() { }
        protected void A3() { }
        public void A4() { }
        static void A5() { }
        public static void A6() { }
    }

    class B : A
    {
        private void B1() { }
        internal void B2() { }
        protected void B3() { }
        public void B4() { }
        static void B5() { }
        public static void B6() { }
    }



    [TestClass]
    public class BasicTest
    {
        [TestMethod]
        public void Initialization()
        {
            var ax = typeof(B).GetMethodsInfo();


            using var serialStream = new SerialStream();
            var schema = SerialStream.GenerateSerializationSchema();
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

            using var ss = new SerialStream();
            ss.Serialize(product);

            ss.Position = 0;
            var newProduct = ss.DeserializeObject<Product>();

            Assert.AreEqual(product.Name, newProduct.Name);
            Assert.AreEqual(product.Price, newProduct.Price);
        }
    }
}
