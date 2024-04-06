using System.Reflection;
using RetroCVR.Properties;

[assembly: AssemblyVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyFileVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyInformationalVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyTitle(nameof(RetroCVR))]
[assembly: AssemblyCompany(AssemblyInfoParams.Author)]
[assembly: AssemblyProduct(nameof(RetroCVR))]

namespace RetroCVR.Properties;
internal static class AssemblyInfoParams {
    public const string Version = "0.0.1";
    public const string Author = "kafeijao";
}
