using System.Diagnostics;
using System.Reflection;
namespace VPS.Helpers
{
    public static class ProjectPropertiesHelper
    {
        public static string? GetProjectVersion()
        {
            var assemblyInfo = Assembly.GetEntryAssembly();
            if (assemblyInfo == null) return null;
            return FileVersionInfo.GetVersionInfo(assemblyInfo.Location).ProductVersion;
        }

        public static string? GetProjectName()
        {
            var assemblyInfo = Assembly.GetEntryAssembly();
            if (assemblyInfo == null) return null;
            return FileVersionInfo.GetVersionInfo(assemblyInfo.Location).ProductName;
        }
    }
}
