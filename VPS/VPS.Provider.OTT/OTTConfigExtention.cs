using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Configurations.OTT;

namespace VPS.Provider.OTT
{
    public static class OTTConfigExtention
    {
        public static IServiceCollection AddOTTVoucherConfigurationSettings(this IServiceCollection services, WebApplicationBuilder builder)
        {
            services.Configure<OttVoucherConfiguration>(builder.Configuration.GetSection("OTTVoucherConfiguration"));
            services.Configure<KafkaQueueConfiguration>(builder.Configuration.GetSection("KafkaQueueConfiguration"));
            services.AddHttpClient();
            return services;
        }
    }
}
