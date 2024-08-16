using Microsoft.Extensions.Configuration;

namespace VPS.ControlCenter.Logic.Helpers
{
    public static class ConfigurationHelper
    {
        public static IConfiguration StaticConfig;
        public static void Initialize(IConfiguration configuration)
        {
            StaticConfig = configuration;
        }
    }
}
