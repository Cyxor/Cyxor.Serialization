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
        void Serialize(SerialStream serializer);
        void Deserialize(SerialStream serializer);
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class CyxorIgnoreAttribute : Attribute { }

    public class Serializable
    {
        [CyxorIgnore]
        SerialStream serializer = new SerialStream();
        public virtual SerialStream Serializer
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
                serializer.DeserializeRawObject(this);
            }
        }
    }

    public interface IBackingSerializer
    {
#if NULLER
        void Serialize(object? value, SerialStream ss, bool rawValue = false);
        TObject Deserialize<TObject>(SerialStream ss, bool rawValue = false);
        object? Deserialize(SerialStream ss, Type type, bool rawValue = false);
#else
        void Serialize(object value, SerialStream ss, bool rawValue = false);
        TObject Deserialize<TObject>(SerialStream ss, bool rawValue = false);
        object Deserialize(SerialStream ss, Type type, bool rawValue = false);
#endif
    }

    public class NullSerializer : IBackingSerializer
    {
#if NULLER
        public void Serialize(object? value, SerialStream ss, bool rawValue = false)
#else
        public void Serialize(object value, SerialStream ss, bool rawValue = false)
#endif
        {
            if (rawValue)
                ss.SerializeRaw(default(object));
            else
                ss.Serialize(default(object));
        }

        public TObject Deserialize<TObject>(SerialStream serializer, bool rawValue = false)
            => rawValue ? serializer.DeserializeRawObject<TObject>() : serializer.DeserializeObject<TObject>();

#if NULLER
        public object? Deserialize(SerialStream ss, Type type, bool rawValue = false)
#else
        public object Deserialize(SerialStream ss, Type type, bool rawValue = false)
#endif
            => rawValue ? ss.DeserializeRawObject(type) : ss.DeserializeObject(type);
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class BackingSerializerAttribute : Attribute
    {
        public static BackingSerializerAttribute Default { get; } = new BackingSerializerAttribute(null);

        public IBackingSerializer BackingSerializer { get; private set; }

        public BackingSerializerAttribute(IBackingSerializer backingSerializer)
        {
            BackingSerializer = backingSerializer;
        }

        public override int GetHashCode() => BackingSerializer.GetHashCode();

        public override bool Equals(object value)
        {
            if (value == this)
                return true;

            var attribute = (value as BackingSerializerAttribute);

            if (attribute != null)
                return BackingSerializer == attribute.BackingSerializer;

            return false;
        }
    }
}