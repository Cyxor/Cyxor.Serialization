namespace Cyxor.Serialization;

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

        foreach (var field in fields) fieldList.Add(new FieldData(field, ShouldSerializeField(field)));

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

    public override int GetHashCode() => HashCode;

    public bool Equals(TypeData other) => HashCode == other.HashCode;

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

        var propertyName = field.Name[1..field.Name.IndexOf('>', StringComparison.Ordinal)];

        var property = field.DeclaringType!.GetPropertyInfo(propertyName)!;

        return ShouldSerializeProperty(property);
    }

    static bool ShouldSerializeProperty(PropertyInfo property) =>
        !property.IsDefined(typeof(CyxorIgnoreAttribute), inherit: false);
}
