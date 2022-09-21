using System;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        public void Serialize(Uri? value) => Serialize(value?.ToString());

        public void SerializeRaw(Uri? value) => SerializeRaw(value?.ToString());
    }
}
