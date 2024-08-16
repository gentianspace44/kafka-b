using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Configurations.BluVoucher;
using VPS.Services.BluVoucher;

namespace VPS.Provider.BluVoucher
{
    public static class BluVoucherConfigExtention
    {
        public static IServiceCollection AddBluVoucherConfigurationSettings(this IServiceCollection services, WebApplicationBuilder builder)
        {

            services.Configure<BluVoucherConfiguration>(builder.Configuration.GetSection("BluVoucherConfiguration"));
            services.Configure<KafkaQueueConfiguration>(builder.Configuration.GetSection("KafkaQueueConfiguration"));
            services.AddScoped<IAirtimeAuthentication, AirtimeAuthentication>();

            return services;
        }
    }
}
