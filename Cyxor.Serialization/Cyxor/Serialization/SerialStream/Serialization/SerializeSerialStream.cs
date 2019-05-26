namespace Cyxor.Serialization
{
    partial class SerialStream
    {
        void InternalSerialize(SerialStream? value, bool raw)
        {
            if (value == default)
            {
                if (!raw)
                    Serialize((byte)0);

                return;
            }

            InternalSerialize(value.buffer, 0, value.length, raw);
        }

        public void Serialize(SerialStream? value)
            => InternalSerialize(value, AutoRaw);

        public void SerializeRaw(SerialStream? value)
            => InternalSerialize(value, raw: true);
    }
}