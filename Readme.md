# Cyxor.Serialization

#### Effective binary object serialization class library for low-overhead network transmissions

## Introduction

**Serialization** is the process of converting objects into a sequence of bytes. The reverse, turning those bytes into objects in memory, is called **Deserialization**.

Serialization serves two main purposes:
 1. Persist object state to a file.
 2. Send data over the network.

We can also say there are two main formats for the task:
 1. Text
 2. Binary

Text format is desirable when you want to produce human-readable output. Binary format on the contrary, can be adapted to produce a more compact output.

Serialization is a well covered topic. There are already a few good articles and libraries available on Internet that you can learn from, each one with it's pros and cons. What has *Cyxor.Serialization* to offer?

Let's take the following class model as example:

```csharp
class Product
{
    public string Name { get; set; }
    public float Price { get; set; }
}
```

If we serialize a given product using `json` we get something like this:

```json
{
    "Name": "PC",
    "Price": 450.00
}
```

It is very common and desirable to include metadata or the structure of the model we are serializing along with the data. But if both sides (producer and consumer, or client and server) shares that model structure, can we serialize only the data: `PC 450.00`?

*Cyxor.Serialization* aims to do exactly that, and we have experienced great results in our network protocols by reducing the amount of data on the wire.

## Using the code

The main class in the library is `SerialStream`. As the name suggest, `SerialStream` is a stream (inherits from `System.IO.Stream`) and serve for both serialization and deserialization.

***[Work In Progress]...***

#### Demo

```CSharp
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

```