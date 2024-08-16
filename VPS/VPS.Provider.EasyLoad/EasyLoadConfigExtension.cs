using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Configurations.EasyLoad;

namespace VPS.Provider.EasyLoad
{
    public static class EasyLoadConfigExtension
    {
        public static IServiceCollection AddEasyLoadConfigurationSettings(this IServiceCollection services, WebApplicationBuilder builder)
        {
            services.Configure<EasyLoadConfiguration>(builder.Configuration.GetSection("EasyLoadConfiguration"));
            services.Configure<KafkaQueueConfiguration>(builder.Configuration.GetSection("KafkaQueueConfiguration"));
            return services;
        }
    }
}
