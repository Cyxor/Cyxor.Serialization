using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Cyxor.Serialization;

partial class Serializer
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull("throwOnNullReference")]
    System.Version? InternalDeserializeNullableVersion(bool raw, bool? throwOnNullReference = null)
    {
        var length = 0;
        long startPosition = 0;

        if (!raw)
        {
            length = InternalDeserializeSequenceHeader();

            if (length == 0)
                return new System.Version();
            else if (length == -1)
            {
                if (throwOnNullReference ?? false)
                    throw new SerializationException(
                        Utilities.ResourceStrings.NullReferenceFoundWhenDeserializingNonNullableReference(
                            typeof(string).Name
                        )
                    );

                return null;
            }

            startPosition = Position;
        }

        var major = DeserializeInt32();
        var minor = DeserializeInt32();
        var build = DeserializeInt32();
        var revision = DeserializeInt32();

        if (length != startPosition)
            throw new SerializationException();

        return new System.Version(major, minor, build, revision);
    }

    public System.Version DeserializeVersion() =>
        InternalDeserializeNullableVersion(AutoRaw, throwOnNullReference: true);

    public System.Version? DeserializeNullableVersion() => InternalDeserializeNullableVersion(raw: false);

    public System.Version DeserializeRawVersion() =>
        InternalDeserializeNullableVersion(raw: true, throwOnNullReference: true);

    public System.Version? DeserializeNullableRawVersion() => InternalDeserializeNullableVersion(raw: true);

    public bool TryDeserializeVersion([NotNullWhen(true)] out System.Version? value)
    {
        if (InternalTryDeserializeSequenceHeader(out var bytes))
        {
            InternalEnsureDeserializeCapacity(5); //TODO
        }

        value = null;
        return false;
    }

    public bool TryDeserializeNullableVersion(out System.Version? value)
    {
        value = default;

        if (!TryDeserializeNullableBytes(out var bytes))
            return false;

        if (bytes == default)
            return true;

        try
        {
            value = new System.Version();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public System.Version ToVersion()
    {
        _position = 0;
        return DeserializeRawVersion();
    }

    public System.Version? ToNullableVersion()
    {
        _position = 0;
        return DeserializeNullableRawVersion();
    }
}
