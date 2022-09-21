using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cyxor.Serialization.Test
{
    [TestClass]
    public class NullableReferences
    {
        [TestMethod]
        public void String()
        {
            using var ss = new Serializer();

            var xx = ss.DeserializeRawObject<string>();

            Assert.IsNotNull(ss);
        }

        public struct Fuera
        {
        }

        [TestMethod]
        public void VayaFuera()
        {
            //var fuera = new Fuera();
            Fuera? fuera = null;

            using var ss = new Serializer();
            ss.SerializeRaw(fuera);

            ss.Position = 0;

            var qq = ss.DeserializeRawObject<Fuera>();

            Assert.AreEqual(fuera, qq);
        }

        enum MyEnum
        {
            v1,
            v2,
        }

        [TestMethod]
        public void VayaGuid()
        {
            MyEnum? guid = null;

            using var ss = new Serializer();
            ss.SerializeRaw(guid);

            ss.Position = 0;

            var qq = ss.DeserializeRawObject<MyEnum?>();
            //var qq = ss.DeserializeNullableObject(typeof(Fuera?));
            //var qq = ss.DeserializeNullableRawObject<Fuera>();

            Assert.AreEqual(guid, qq);
        }

        [TestMethod]
        public void MString()
        {
            string? dd = null;

            using var ss = new Serializer();
            ss.SerializeRaw(dd);

            ss.Position = 0;
            var qq = ss.DeserializeNullableRawObject<string>();

            Assert.AreEqual(dd, qq);
        }

        [TestMethod]
        public void TestNI()
        {
            int? ni = default;

            using var ss = new Serializer();
            ss.Serialize(ni);

            ss.Position = 0;
            var qq = ss.DeserializeObject<int?>();
            ss.Position = 0;
            var ww = ss.DeserializeObject<int?>();

            Assert.AreEqual(ni, qq);
            Assert.AreEqual(ni, ww);
        }

        [TestMethod]
        public void Wester()
        {
            Fuera? fuera = new Fuera();

            using var ss = new Serializer();
            ss.Serialize(fuera);

            ss.Position = 0;

            var qq = ss.DeserializeNullableValue<Fuera>();

            Assert.AreEqual(fuera, qq);
        }
    }
}
