namespace Cyxor.Serialization
{
    partial class SerializationStream
    {
        static class ObjectProperties
        {
            public const byte MaxLength = 0b_00_111111;
            public const byte LengthMap = 0b_111111_00;
            public const byte EmptyMap = 0b_000000_01;
            public const byte PartialMap = 0b_000000_10;
            public const byte CircularMap = 0b_000000_11;

            //public static bool IsNull(byte value)
            //    => value == 0;

            //public static bool IsNotNull(byte value)
            //    => value != 0;

            //public static bool IsEmpty(byte value)
            //    => (value & EmptyMap) == EmptyMap;

            //public static bool IsPartial(byte value)
            //    => (value & PartialMap) == PartialMap;

            //public static bool IsCircular(byte value)
            //    => (value & CircularMap) == CircularMap;

            //public static bool IsLength(byte value)
            //    => value < LengthMap;

            public static int Length(byte value)
                => (value & LengthMap) >> 2;

            //public static void SerializeOp(SerialStream serialStream, int count)
            //{
            //    if (count < ObjectProperties.MaxLength)
            //        serialStream.Serialize((byte)(count << 2));
            //    else
            //    {
            //        serialStream.Serialize(ObjectProperties.LengthMap);
            //        serialStream.Serialize(count);
            //    }
            //}

            //public static int DeserializeOp(SerialStream serialStream)
            //{
            //    var op = serialStream.DeserializeByte();

            //    return op == 0 ? -1
            //        : op == ObjectProperties.EmptyMap ? 0
            //        : op == ObjectProperties.LengthMap ? serialStream.DeserializeInt32()
            //        : ObjectProperties.Length(op);
            //}
        }
    }
}
