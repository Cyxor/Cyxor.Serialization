using System.Reflection;

/// <summary>
/// Effective binary object serialization class library for low-overhead network transmissions.
/// </summary>
namespace Cyxor.Serialization;
public static class About
{
    public static string? Version =>
        Utilities.Reflection.GetCustomAssemblyAttribute<AssemblyInformationalVersionAttribute>(
            typeof(About)
        )?.InformationalVersion
        ?? typeof(About).GetTypeInfo().Assembly.GetName().Version?.ToString();

    public static string Description =>
        "Effective binary object serialization class library for low-overhead network transmissions";
}
