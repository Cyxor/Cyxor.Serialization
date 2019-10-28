//#if !NET20 && !NET35 && !NET40 && !NETSTANDARD1_0

//using System;

//namespace Cyxor.Serialization
//{
//    partial class SerialStream
//    {
//        public void Serialize<T>(Memory<T> memory) where T : struct
//            => InternalSerialize((ReadOnlySpan<T>)memory.Span, AutoRaw);

//        public void SerializeRaw<T>(Memory<T> memory) where T : struct
//            => InternalSerialize((ReadOnlySpan<T>)memory.Span, raw: true);

//        public void Serialize<T>(ReadOnlyMemory<T> readOnlyMemory) where T : struct
//            => InternalSerialize(readOnlyMemory.Span, AutoRaw);

//        public void SerializeRaw<T>(ReadOnlyMemory<T> readOnlyMemory) where T : struct
//            => InternalSerialize(readOnlyMemory.Span, raw: true);

//        public void Serialize<T>(Memory<T>? memory) where T : struct
//            => SerializeNullableValue(memory, Serialize);

//        public void Serialize<T>(ReadOnlyMemory<T>? readOnlyMemory) where T : struct
//            => SerializeNullableValue(readOnlyMemory, Serialize);
//    }
//}

//#endif