using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Configurations.EasyLoad;
using VPS.Domain.Models.Configurations.Flash;
namespace VPS.Provider.Flash
{
    public static class FlashConfigExtention
    {
        public static IServiceCollection AddFlashConfigurationSettings(this IServiceCollection services, WebApplicationBuilder builder)
        {
           
            services.Configure<KafkaQueueConfiguration>(builder.Configuration.GetSection("KafkaQueueConfiguration"));
            return services;
        }
    }
}
