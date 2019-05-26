using System.Reflection;

/// <summary>
/// Effective binary object serialization class library for low-overhead network transmissions.
/// </summary>
namespace Cyxor.Serialization
{
    using Extensions;

    public static class Version
    {
        public static string? Value => Utilities.Reflection.GetCustomAssemblyAttribute
            <AssemblyInformationalVersionAttribute>(typeof(Version))?.InformationalVersion ??
            typeof(Version).GetTypeInfo().Assembly.GetName().Version?.ToString();
    }
}