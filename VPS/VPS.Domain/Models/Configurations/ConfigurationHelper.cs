using Microsoft.Extensions.Configuration;

namespace VPS.Domain.Models.Configurations
{
    public static class ConfigurationHelper
    {
        private static IConfiguration? _staticConfig;
        public static void Initialize(IConfiguration configuration)
        {
            _staticConfig = configuration;
        }

        public static IConfiguration StaticConfig { 
            get {
                return _staticConfig!;
            } 
        }

    }
}
