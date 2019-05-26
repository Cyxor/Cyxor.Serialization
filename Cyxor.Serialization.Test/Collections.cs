using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cyxor.Serialization.Test
{
    [TestClass]
    public class Collections
    {
        enum ProductCategory
        {
            Computers,
            Vehicles
        }

        sealed class Product : IEquatable<Product>
        {
            public string Name { get; set; } = string.Empty;
            public float Price { get; set; }
            public ProductCategory Category { get; set; }

            public override int GetHashCode()
                => $"{Name}{Price}{Category}".GetHashCode();

            public override bool Equals(object? obj)
                => obj is Product other ? Equals(other) : false;

            public bool Equals(Product other)
            {
                var result = Category.Equals(other.Category);
                result &= Name.Equals(other.Name);
                result &= Price.Equals(other.Price);

                return result;
            }
        }

        [TestMethod]
        public void GroupingX()
        {
            using var ss = new SerialStream();
            ss.Serialize(ProductCategory.Vehicles);

            ss.Position = 0;
            var xc = (ProductCategory)ss.DeserializeObject(typeof(ProductCategory));

            Assert.IsTrue(ProductCategory.Vehicles == xc);
        }

        [TestMethod]
        public void Grouping()
        {
            var products = new List<Product>
            {
                new Product { Name = "PC", Price = 450.00f, Category = ProductCategory.Computers },
                new Product { Name = "Monitor", Price = 180.99f, Category = ProductCategory.Computers },
                new Product { Name = "Printer", Price = 180.99f, Category = ProductCategory.Computers },
                new Product { Name = "Car", Price = 12749.00f, Category = ProductCategory.Vehicles },
            };

            var productsGroupedByCategory = products.GroupBy(p => p.Category);

            var productsGroupedByCategoryThenByPrice = from product in products
                                                       group product by product.Category into categoryGroup
                                                       from categoryProduct in
                                                           from product in categoryGroup
                                                           group product by product.Price
                                                       group categoryProduct by categoryGroup.Key;

            using var serializer = new SerialStream();
            serializer.Serialize(productsGroupedByCategory.First());
            serializer.Serialize(productsGroupedByCategory);
            serializer.Serialize(productsGroupedByCategoryThenByPrice);

            using var deserializer = new SerialStream(serializer) { Position = 0 };

            //todo: error in the deserialization of the enum.

            var deserializedProductsGroupedByFirstCategory = deserializer.DeserializeIGrouping<ProductCategory, Product>();

            TestIGrouping(productsGroupedByCategory.First(), deserializedProductsGroupedByFirstCategory);

            var deserializedProductsGroupedByCategory = deserializer.DeserializeIEnumerable<IGrouping<ProductCategory, Product>>();

            TestIEnumerableIGrouping(productsGroupedByCategory, deserializedProductsGroupedByCategory);

            var deserializedProductsGroupedByCategoryThenByPrice = deserializer.DeserializeIEnumerable<IGrouping<ProductCategory, IGrouping<float, Product>>>();

            TestIEnumerableIGroupingIGrouping(productsGroupedByCategoryThenByPrice, deserializedProductsGroupedByCategoryThenByPrice);

            static void TestIEnumerableIGroupingIGrouping<TFirstKey, TSecondKey, TElement>(IEnumerable<IGrouping<TFirstKey, IGrouping<TSecondKey, TElement>>> first, IEnumerable<IGrouping<TFirstKey, IGrouping<TSecondKey, TElement>>> second)
                //where TFirstKey: notnull
                //where TSecondKey: notnull
            {
                TestIEnumerableIGrouping(first, second);

                for (var i = 0; i < first.Count(); i++)
                    TestIEnumerableIGrouping(first.ElementAt(i), second.ElementAt(i));
            }

            static void TestIEnumerableIGrouping<TKey, TElement>(IEnumerable<IGrouping<TKey, TElement>> first, IEnumerable<IGrouping<TKey, TElement>> second)
                //where TKey: notnull
            {
                var firstCount = first.Count();

                Assert.AreEqual(firstCount, second.Count());

                for (var i = 0; i < firstCount; i++)
                {
                    var firstElementAti = first.ElementAt(i);
                    var secondElementAti = second.ElementAt(i);

                    Assert.AreEqual(firstElementAti.Key, secondElementAti.Key);

                    if (typeof(TElement).Name != typeof(IGrouping<,>).Name)
                        TestIGrouping(firstElementAti, secondElementAti);
                }
            }

            static void TestIGrouping<TKey, TElement>(IGrouping<TKey, TElement> first, IGrouping<TKey, TElement> second)
                //where TKey: notnull
            {
                var firstCount = first.Count();

                Assert.AreEqual(first.Key, second.Key);
                Assert.AreEqual(firstCount, second.Count());

                for (var i = 0; i < firstCount; i++)
                    Assert.IsTrue(first.ElementAt(i)!.Equals(second.ElementAt(i)));
            }
        }
    }
}
