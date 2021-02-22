//using System;
//using System.Linq;
//using System.Threading;
//using System.Runtime.InteropServices;
//using System.Runtime.CompilerServices;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cyxor.Serialization.Test
{
    [TestClass]
    public class ValueTypes
    {
        public void Serialize<T>(T t = default)
        {
            //    using var serializer = new Serializer();
            //    serializer.Serialize(t);

            //    var currentLength = serializer.Length;
            //    var currentPosition = serializer.Position;

            //    serializer.Position = 0;

            //    var value = serializer.Deserialize<T>();

            //    Assert.AreEqual(value, t);
            //    Assert.AreEqual(currentLength, serializer.Length);
            //    Assert.AreEqual(currentPosition, serializer.Position);
        }

        [TestMethod]
        public void SerializeBoolean()
        {
            const bool ValueTrue = true;
            const bool ValueFalse = false;

            Serialize(ValueTrue);
            Serialize(ValueFalse);
            Serialize<bool>();

            using var serializer = new Serializer();
            serializer.Serialize(ValueTrue);
            serializer.Serialize(ValueFalse);

            var currentLength = serializer.Length;
            var currentPosition = serializer.Position;

            serializer.Position = 0;

            var valueTrue = serializer.DeserializeBoolean();
            var valueFalse = serializer.DeserializeBoolean();

            Assert.IsTrue(valueTrue);
            Assert.IsFalse(valueFalse);
            Assert.AreEqual(currentLength, serializer.Length);
            Assert.AreEqual(currentPosition, serializer.Position);
        }
    }
}
