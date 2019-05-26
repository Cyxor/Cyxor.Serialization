using System;

namespace Cyxor.Serialization
{
    partial class SerialStream
    {
        public void Serialize(Uri? value)
            => Serialize(value?.ToString());

        public void SerializeRaw(Uri? value)
            => SerializeRaw(value?.ToString());
    }
}