
namespace VPS.Domain.Models.Configurations
{
    public class CustomLoggingConfiguration
    {
        public ElasticConfig? ElasticConfig { get; set; } 
        
        public string GrafanaLokiEndpoint { get; set; } = string.Empty;
    }

    public class ElasticConfig
    {
        public string NodeUri { get; set; } = string.Empty;
        public string IndexFormat { get; set; } = string.Empty;
        public string ElasticUserName { get; set; } = string.Empty;
        public string ElasticPassword { get; set; } = string.Empty;

    }
}
