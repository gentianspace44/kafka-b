using VPS.Domain.Models.Configurations;

namespace VPS.Provider.HollyTopUp
{
    public static class HollyTopUpConfigExtention
    {
        public static IServiceCollection AddHollyTopUpConfigExtention(this IServiceCollection services, WebApplicationBuilder builder)
        {
            services.Configure<KafkaQueueConfiguration>(builder.Configuration.GetSection("KafkaQueueConfiguration"));
            services.AddHttpClient();
            return services;
        }
    }
}
