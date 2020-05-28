using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Cyxor.Extensions;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Cyxor.Serialization.Test
{
    [TestClass]
    public class SerializeValueTypes
    {
        [TestMethod]
        public void SerializeBoolean()
        {
            using var serializer = new Serializer();
            serializer.Serialize(true);
            serializer.Serialize(false);
            //Assert.IsTrue(schema.Length > 0);
        }
    }
}
