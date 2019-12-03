using System;
using System.Reflection;
using System.Collections.Generic;

namespace Cyxor.Serialization
{
    using Extensions;

    sealed class TypeData : IEquatable<TypeData>
    {
        readonly int HashCode;
        public TypeData? Parent;
        public readonly Type Type;
        public readonly FieldData[] Fields;
        public readonly PropertyData[] Properties;

        public TypeData(Type type)
        {
            Type = type;

            #region Fields
            var fields = type.GetFieldsInfo(inheritedFields: false);

            var fieldList = new List<FieldData>();

            foreach (var field in fields)
            {
                var shouldSerialize = ShouldSerializeField(field);
                //fieldList.Add(new FieldData(field, shouldSerialize));

                // TODO: If the field is static the expressions will fail
                if (shouldSerialize)
                    fieldList.Add(new FieldData(field, shouldSerialize));
            }

            Fields = fieldList.ToArray();
            Array.Sort(Fields);
            #endregion

            #region Properties
            var properties = type.GetPropertiesInfo(inheritedProperties: false, publicGetAccessor: true);

            var propertyList = new List<PropertyData>();

            foreach (var property in properties)
            {
                var shouldSerialize = ShouldSerializeProperty(property);

                if (shouldSerialize)
                    propertyList.Add(new PropertyData(property, shouldSerialize));
            }

            Properties = propertyList.ToArray();
            Array.Sort(Properties);
            #endregion

            if (type.FullName == default)
                throw new InvalidOperationException(Utilities.ResourceStrings.CyxorInternalException);

            HashCode = Utilities.HashCode.GetFrom(type.FullName);
        }

        public override int GetHashCode()
            => HashCode;

        public bool Equals(TypeData other)
            => HashCode == other.HashCode;

        public override bool Equals(object? obj)
        {
            if (obj == default)
                return false;

            var typeData = obj as TypeData;

            return typeData == default ? false : Equals(typeData);
        }

        static bool ShouldSerializeField(FieldInfo field)
        {
            if (field.IsDefined(typeof(CyxorIgnoreAttribute), inherit: false))
                return false;

            if (field.IsStatic && field.IsInitOnly)
                return false;

            if (field.Name[0] != '<')
                return true;

#pragma warning disable IDE0057 // Substring can be simplified
            var propertyName = field.Name.Substring(1, field.Name.IndexOf('>', StringComparison.Ordinal) - 1);
#pragma warning restore IDE0057 // Substring can be simplified

            var property = field.DeclaringType!.GetPropertyInfo(propertyName)!;

            return ShouldSerializeProperty(property);
        }

        static bool ShouldSerializeProperty(PropertyInfo property)
            => !property.IsDefined(typeof(CyxorIgnoreAttribute), inherit: false);
    }
}