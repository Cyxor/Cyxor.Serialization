namespace Cyxor.Serialization
{
    partial class SerialStream
    {
        void InternalSerialize(string? value, bool raw)
        {
            if (value == default)
            {
                if (!raw)
                    Serialize((byte)0);

                return;
            }

            unsafe
            {
                fixed (char* ptr = value)
                    InternalSerialize(ptr, value!.Length, 0, value!.Length, false, false, raw);
            }
        }

        public void Serialize(string? value)
            => InternalSerialize(value, AutoRaw);

        public void SerializeRaw(string? value)
            => InternalSerialize(value, raw: true);
    }
}