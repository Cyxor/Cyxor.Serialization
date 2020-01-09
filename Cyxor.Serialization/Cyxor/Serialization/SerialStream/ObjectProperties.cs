namespace Cyxor.Serialization
{
    partial class Serializer
    {
        static class ObjectProperties
        {
            public const byte MaxLength = 0b_00_111111;
            public const byte LengthMap = 0b_111111_00;
            public const byte EmptyMap = 0b_000000_01;
            public const byte PartialMap = 0b_000000_10;
            public const byte CircularMap = 0b_000000_11;
        }
    }
}
