using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cyxor.Serialization.Test
{
    class Product
    {
        public string Name { get; set; }
        public float Price { get; set; }
    }

    [TestClass]
    public class BasicTest
    {
        [TestMethod]
        public void SimpleObject()
        {
            var product = new Product { Name = "PC", Price = 450.00f };

            var ss = new SerialStream();
            ss.Serialize(product);

            ss.Position = 0;
            var newProduct = ss.DeserializeObject<Product>();

            Assert.AreEqual(product.Name, newProduct.Name);
            Assert.AreEqual(product.Price, newProduct.Price);
        }
    }
}
