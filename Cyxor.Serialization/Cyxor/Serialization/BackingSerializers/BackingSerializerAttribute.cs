using System;
using System.Collections.Generic;
using System.Text;

namespace Cyxor.Serialization
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class BackingSerializerAttribute : Attribute
    {
        public IBackingSerializer BackingSerializer { get; private set; }
        public object? BackingSerializerOptions { get; private set; }

        public BackingSerializerAttribute(IBackingSerializer backingSerializer, object? backingSerializerOptions)
        {
            BackingSerializer = backingSerializer;
            BackingSerializerOptions = backingSerializerOptions;
        }

        public override int GetHashCode()
            => BackingSerializer.GetHashCode();

        public override bool Equals(object? value)
            => value == this
                ? true
                : value is BackingSerializerAttribute attribute
                    ? BackingSerializer == attribute.BackingSerializer
                    : false;
    }
}
