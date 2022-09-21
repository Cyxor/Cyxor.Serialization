using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Cyxor.Serialization
{
    static partial class Utilities
    {
        public static class ResourceStrings
        {
            internal const string ExceptionNegativeNumber = "Non-negative number required",
                CyxorInternalException = "Cyxor internal exception",
                ExceptionFormat = "Cyxor..{0}.{1}() : {2}",
                ExceptionFormat1 = "Cyxor..{0}.{1}({2}) : {3}",
                ExceptionFormat2 = "Cyxor..{0}.{1}({2}, {3}) : {4}",
                ExceptionFormat3 = "Cyxor..{0}.{1}({2}, {3}, {4}) : {5}",
                ExceptionFormat4 = "Cyxor..{0}.{1}({2}, {3}, {4}) : {5}",
                ExceptionMessageBufferDeserializeNumeric = "",
                ExceptionMessageBufferDeserializeObject =
                    "Deserialization operation do not match format of bytes written in the Serialization process.";

            //NullReferenceFoundWhenDeserializingNonNullableReference = "Null reference found when trying to deserialize non-nullable reference object";

            public static string CantCreateInstanceOfType(string typeName) =>
                $"Can't create instance of type '{typeName}'";

            public static string UnableToCastByteArrayToTArray(string typeName) =>
                $"Unable to cast from byte array to array of type '{typeName}'";

            public static string UnableToCastCharArrayToTArray(string typeName) =>
                $"Unable to cast from char array to array of type '{typeName}'";

            public static string TheWholeSerialStreamContentIsNotAnObjectOfType(string typeName) =>
                $"The whole {nameof(Serializer)} content is not an object of type '{typeName}'";

            public static string NullKeyWhenDeserializingDictionary(string keyTypeName, string valueTypeName) =>
                $"Null key when deserializing (IEnumerable<KeyValuePair<{keyTypeName}, {valueTypeName}>>)";

            public static string NullValueFoundWhenDeserializingNonNullableValue(string typeName) =>
                $"Null value found while trying to deserialize non-nullable value of type '{typeName}'. "
                + "Consider deserialize a nullable value type.";

            public static string NullReferenceFoundWhenDeserializingNonNullableReference(string typeName) =>
                $"Null reference found while trying to deserialize non-nullable reference of type '{typeName}'. "
                + "Consider deserialize a nullable reference type.";

            public static string UnableToDeserializeValueTypeAsNullableReference(string typeName) =>
                $"Unable to deserialize value type '{typeName}' as nullable reference."
                + "Only reference types can be used as target when deserializing nullable objects."
                + "Retry the operation using the overload without the 'Nullable' keyword.";

            public static string NullReferenceFoundWhenDeserializingValueType(string typeName) =>
                $"Null reference found while trying to deserialize ref struct of type '{typeName}'.";
        }
    }
}
