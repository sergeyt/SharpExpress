using System.Reflection;

[assembly: AssemblyVersionAttribute(VersionInfo.Version)]
[assembly: AssemblyFileVersionAttribute(VersionInfo.Version)]
[assembly: AssemblyInformationalVersionAttribute(VersionInfo.Version)]

internal static class VersionInfo
{
	public const string Version = "0.4.0.0";
}