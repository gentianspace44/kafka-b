using Microsoft.Extensions.DependencyInjection;
using VPS.Services.Common;

namespace VPS.Services.CustomServiceExtentions
{
    public static class ProviderServiceExtensions
    {
        public static async Task<IServiceCollection> InitializeProviders(this IServiceCollection services)
        {
            services.AddScoped<IVoucherProviderService, VoucherProviderService>();
            var serviceProvider = services.BuildServiceProvider();
            var voucherProviderService = serviceProvider.GetRequiredService<IVoucherProviderService>();
            await voucherProviderService.SetProviders();
            return services;
        }
    }
}