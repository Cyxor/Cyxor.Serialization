namespace Cyxor.Serialization
{
    partial class SerializationStream
    {
        void InternalSerialize(SerializationStream? value, bool raw)
        {
            if (value == default)
            {
                if (!raw)
                    Serialize((byte)0);

                return;
            }

            InternalSerialize(value.buffer, 0, value.length, raw);
        }

        public void Serialize(SerializationStream? value)
            => InternalSerialize(value, AutoRaw);

        public void SerializeRaw(SerializationStream? value)
            => InternalSerialize(value, raw: true);
    }
}