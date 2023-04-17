using System.Diagnostics.CodeAnalysis;

namespace Cyxor.Serialization;

partial class Serializer
{
    public Uri DeserializeUri() => new Uri(DeserializeString());

    public Uri? DeserializeNullableUri()
    {
        var str = DeserializeNullableString();
        return str == default ? default : new Uri(str);
    }

    public Uri DeserializeRawUri() => new Uri(DeserializeRawString());

    public Uri? DeserializeNullableRawUri()
    {
        var str = DeserializeNullableStringRaw();
        return str == default ? default : new Uri(str);
    }

    public bool TryDeserializeUri([NotNullWhen(true)] out Uri? value)
    {
        value = default;

        if (!TryDeserializeString(out var str))
            return false;

        try
        {
            value = new Uri(str);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool TryDeserializeNullableUri(out Uri? value)
    {
        value = default;

        if (!TryDeserializeNullableString(out var str))
            return false;

        if (str == default)
            return true;

        try
        {
            value = new Uri(str);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public Uri ToUri()
    {
        _position = 0;
        return DeserializeRawUri();
    }

    public Uri? ToNullableUri()
    {
        _position = 0;
        return DeserializeNullableRawUri();
    }
}
