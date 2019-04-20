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
            public string Name { get; set; }
            public float Price { get; set; }
            public ProductCategory Category { get; set; }

            public override int GetHashCode()
                => $"{Name}{Price}{Category}".GetHashCode();

            public override bool Equals(object obj)
                => Equals(obj as Product);

            public bool Equals(Product other)
            {
                var result = Category.Equals(other.Category);
                result &= Name.Equals(other.Name);
                result &= Price.Equals(other.Price);

                return result;
            }
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

            var ss = new SerialStream();
            ss.Serialize(productsGroupedByCategory.First());
            ss.Serialize(productsGroupedByCategory);
            ss.Serialize(productsGroupedByCategoryThenByPrice);

            var bytes = ss.ToByteArray();
            ss = new SerialStream(bytes);

            var deserializedProductsGroupedByFirstCategory = ss.DeserializeIGrouping<ProductCategory, Product>();

            TestIGrouping(productsGroupedByCategory.First(), deserializedProductsGroupedByFirstCategory);

            var deserializedProductsGroupedByCategory = ss.DeserializeIEnumerable<IGrouping<ProductCategory, Product>>();

            TestIEnumerableIGrouping(productsGroupedByCategory, deserializedProductsGroupedByCategory);

            var deserializedProductsGroupedByCategoryThenByPrice = ss.DeserializeIEnumerable<IGrouping<ProductCategory, IGrouping<float, Product>>>();

            TestIEnumerableIGroupingIGrouping(productsGroupedByCategoryThenByPrice, deserializedProductsGroupedByCategoryThenByPrice);

            void TestIEnumerableIGroupingIGrouping<TFirstKey, TSecondKey, TElement>(IEnumerable<IGrouping<TFirstKey, IGrouping<TSecondKey, TElement>>> first, IEnumerable<IGrouping<TFirstKey, IGrouping<TSecondKey, TElement>>> second)
            {
                TestIEnumerableIGrouping(first, second);

                for (var i = 0; i < first.Count(); i++)
                    TestIEnumerableIGrouping(first.ElementAt(i), second.ElementAt(i));
            }

            void TestIEnumerableIGrouping<TKey, TElement>(IEnumerable<IGrouping<TKey, TElement>> first, IEnumerable<IGrouping<TKey, TElement>> second)
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

            void TestIGrouping<TKey, TElement>(IGrouping<TKey, TElement> first, IGrouping<TKey, TElement> second)
            {
                var firstCount = first.Count();

                Assert.AreEqual(first.Key, second.Key);
                Assert.AreEqual(firstCount, second.Count());

                for (var i = 0; i < firstCount; i++)
                    Assert.IsTrue(first.ElementAt(i).Equals(second.ElementAt(i)));
            }
        }
    }
}
