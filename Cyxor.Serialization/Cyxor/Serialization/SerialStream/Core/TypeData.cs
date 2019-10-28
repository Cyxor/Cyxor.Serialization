using System;
using System.Collections.Generic;

namespace Cyxor.Serialization
{
    using Extensions;

    partial class SerialStream
    {
        static partial class Reflector
        {
            internal sealed class TypeData : IEquatable<TypeData>
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
                    var fields = type.GetFields();

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
                    var properties = type.GetProperties(isPublic: true);

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
            }
        }
    }
}