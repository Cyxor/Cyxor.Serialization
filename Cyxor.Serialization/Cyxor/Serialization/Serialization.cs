using System;

namespace Cyxor.Serialization
{
    public enum ByteOrder
    {
        LittleEndian,
        BigEndian,
    }

    public interface ISerializable
    {
        void Serialize(SerializationStream serializer);
        void Deserialize(SerializationStream serializer);
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class CyxorIgnoreAttribute : Attribute { }

    public class Serializable : IDisposable
    {
        bool disposed;

        [CyxorIgnore]
        SerializationStream serializer = new SerializationStream();
        public virtual SerializationStream Serializer
        {
            get
            {
                serializer.Reset();
                serializer.SerializeRaw(this);
                return serializer;
            }
            set
            {
                serializer = value;
                serializer.Position = 0;
                _ = serializer.DeserializeRawObject(this);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
                serializer.Dispose();

            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}