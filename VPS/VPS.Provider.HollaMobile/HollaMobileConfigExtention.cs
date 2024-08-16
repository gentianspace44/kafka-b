namespace VPS.Provider.HollaMobile
{
    public static class HollaMobileConfigExtention
    {
        public static IServiceCollection AddHollaMobileConfigExtention(this IServiceCollection services, WebApplicationBuilder builder)
        {
            services.AddHttpClient();
            return services;
        }
    }
}
