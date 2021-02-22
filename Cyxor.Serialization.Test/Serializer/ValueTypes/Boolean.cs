using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cyxor.Serialization.Test
{
    class BooleanModel
    {
        public bool ValueTrue { get; set; } = true;
        public bool ValueFalse { get; set; } = false;
        public bool? NullableValueNull { get; set; } = null;
        public bool? NullableValueTrue { get; set; } = true;
        public bool? NullableValueFalse { get; set; } = false;
    }

    [TestClass]
    public class Boolean
    {
        [TestMethod]
        public void Test()
        {
            var valueTrue = true;
            var valueFalse = false;
            bool? nullableValueNull = null;
            bool? nullableValueTrue = true;
            bool? nullableValueFalse = false;
            var booleanModel = new BooleanModel();

            var nullArray = default(bool[]);
            var emptyArray = Array.Empty<bool>();
            var valueArray = new bool[] { true, false, false, true, false };
            var nullableValueArray = new bool?[] { true, null, false, true, false };

            //var nullIEnumerable = default(Queue<bool>);
            //var emptyIEnumrable = new ConcurrentStack<bool>();
            //var valueArray = new bool[] { true, false, false, true, false };
            //var nullableValueArray = new bool?[] { true, null, false, true, false };

            using var serializer = new Serializer();

            serializer.Serialize(valueTrue);
            serializer.Serialize(valueFalse);
            serializer.Serialize(nullableValueNull);
            serializer.Serialize(nullableValueTrue);
            serializer.Serialize(nullableValueFalse);
            serializer.Serialize(booleanModel);
        }
    }
}
