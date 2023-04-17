namespace Cyxor.Serialization;

partial class Serializer
{
    public Serializer DeserializeSerialStream() => new Serializer(DeserializeBytes());

    public Serializer? DeserializeNullableSerialStream()
    {
        var bytes = DeserializeNullableBytes();
        return bytes == default ? default : new Serializer(bytes);
    }

    public Serializer DeserializeRawSerialStream() => new Serializer(DeserializeRawBytes());

    public Serializer? DeserializeNullableRawSerialStream()
    {
        var bytes = DeserializeNullableRawBytes();
        return bytes == default ? default : new Serializer(bytes);
    }

    public bool TryDeserializeSerialStream([NotNullWhen(true)] out Serializer? value)
    {
        value = default;

        if (!TryDeserializeBytes(out var bytes))
            return false;

        try
        {
            value = new Serializer(bytes);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool TryDeserializeNullableSerialStream(out Serializer? value)
    {
        value = default;

        if (!TryDeserializeNullableBytes(out var bytes))
            return false;

        if (bytes == default)
            return true;

        try
        {
            value = new Serializer(bytes);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public Serializer ToSerialStream()
    {
        _position = 0;
        return DeserializeRawSerialStream();
    }

    public Serializer? ToNullableSerialStream()
    {
        _position = 0;
        return DeserializeNullableRawSerialStream();
    }
}
