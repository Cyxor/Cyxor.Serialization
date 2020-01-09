namespace Cyxor.Serialization
{
    partial class Serializer
    {
        void InternalSerialize(Serializer? value, bool raw)
        {
            if (value == default)
            {
                if (!raw)
                    Serialize((byte)0);

                return;
            }

            InternalSerialize(value.buffer, 0, value.length, raw);
        }

        public void Serialize(Serializer? value)
            => InternalSerialize(value, AutoRaw);

        public void SerializeRaw(Serializer? value)
            => InternalSerialize(value, raw: true);
    }
}