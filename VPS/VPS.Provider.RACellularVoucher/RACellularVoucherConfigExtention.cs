using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Configurations.RACellularVoucher;

namespace VPS.Provider.RACellularVoucher;

public static class RACellularVoucherConfigExtention
{
    public static IServiceCollection AddRACellularConfigurationSettings(this IServiceCollection services, WebApplicationBuilder builder)
    {
        services.Configure<RACellularVoucherConfiguration>(builder.Configuration.GetSection("RACellularVoucherConfiguration"));
        services.Configure<KafkaQueueConfiguration>(builder.Configuration.GetSection("KafkaQueueConfiguration"));

        return services;
    }
}

