using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prometheus;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.Grafana.Loki;
using VPS.API.Common;
using VPS.API.Syx;
using VPS.Domain.Models.Configurations;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Infrastructure.Repository.Redis;
using VPS.Services.Common;
using VPS.Services.Common.HealthCheck;
using VPS.Services.Kafka;

namespace VPS.Services.CustomServiceExtentions
{
    public static class DefaultServiceExtentions
    {
        public static IServiceCollection AddDefaultServicesForProviders(this IServiceCollection services, IConfiguration configuration)
        {
            ConfigurationHelper.Initialize(configuration);
            services.AddTransient(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>));
            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.AddTransient<IHttpClientWrapper, HttpClientWrapper>();
            services.AddTransient<IHttpClientCommunication, HttpClientCommunication>();
            services.AddScoped<ITcpClient, TcpClient>();
            services.AddScoped<IClientBonusService, ClientBonusService>();
            services.AddScoped<IReferenceGeneratorService, ReferenceGeneratorService>();
            services.AddScoped<IEligibleBonusRepository, EligibleBonusRepository>();
            services.AddScoped<IGetStreamResults, GetStreamResults>();
            services.AddScoped<IRedisRepository, RedisRepository>();
            services.AddScoped<IVoucherLogRepository, VoucherLogRepository>();
            services.AddScoped<IRedisService, RedisService>();
            services.AddScoped<IVoucherBatchProcessingRepository, VoucherBatchProcessingRepository>();
            services.AddScoped<IVoucherValidationService, VoucherValidationService>();
            services.AddScoped<IDBHealthCheckRepository, DBHealthCheckRepository>();
            services.AddScoped<ISyxApiService, SyxApiService>();
            services.AddSingleton<RedisStore>();
            services.AddTransient<ExceptionMiddleware>();
            services.AddSingleton<MetricsHelper>();

            services.Configure<DbConnectionStrings>(configuration.GetSection("ConnectionStrings"));
            services.Configure<RedisSettings>(configuration.GetSection("RedisSettings"));
            services.Configure<SyxSettings>(configuration.GetSection("SyxSettings"));
            services.Configure<DBSettings>(configuration.GetSection("DBSettings"));
            services.Configure<VpsControlCenterEndpoints>(configuration.GetSection("VPSControlCenterEndpoints"));
            services.Configure<VoucherRedeemClientNotifications>(configuration.GetSection("VoucherRedeemClientNotifications"));
            services.Configure<CountrySettings>(configuration.GetSection("CountrySettings"));

            services.AddHealthChecks()
            .AddCheck<RedisHealthCheck>(nameof(RedisHealthCheck))
            .AddCheck<SqlHealthCheck>(nameof(SqlHealthCheck))
            .AddCheck<SyxHealthCheck>(nameof(SyxHealthCheck))
            .ForwardToPrometheus();


            return services;
        }


        public static IServiceCollection AddDefaultKafkaService(this IServiceCollection services)
        {
            services.AddSingleton<IVpsKafkaProducer, VpsKafkaProducer>();

            services.AddHealthChecks()
            .AddCheck<KafkaHealthCheck>(nameof(KafkaHealthCheck))
            .ForwardToPrometheus();

            services.AddHostedService<VpsKafkaConsumer>();

            return services;
        }


        public static WebApplicationBuilder AddCustomLogger(this WebApplicationBuilder builder, Domain.Models.Enums.VpsConfiguration providerEnum)
        {

            var loggerConfig = builder.Configuration.GetSection("CustomLoggingConfiguration").Get<CustomLoggingConfiguration>();

            builder.Services.Configure<VpsLoggerConfiguration>(config =>
            {
                config.Provider = providerEnum;
            });

            List<LokiLabel> grafanaLabel = new()
            {
                new()
                {
                    Key = "provider",
                    Value = Enum.GetName(providerEnum)?.ToLower()?? ""
                 }
            };

            Log.Logger = new Serilog.LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information, formatter: new CompactJsonFormatter())
                .WriteTo.GrafanaLoki(loggerConfig?.GrafanaLokiEndpoint?? "", textFormatter: new CompactJsonFormatter(), labels: grafanaLabel)
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(loggerConfig?.ElasticConfig?.NodeUri?? ""))
                {
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                    ModifyConnectionSettings = x => x.BasicAuthentication(loggerConfig?.ElasticConfig?.ElasticUserName, loggerConfig?.ElasticConfig?.ElasticPassword),
                    IndexFormat = loggerConfig?.ElasticConfig?.IndexFormat
                })
                .CreateLogger();

            builder.Services.AddSingleton<ILoggerFactory>(new SerilogLoggerFactory(Log.Logger));
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(Log.Logger);

            return builder;
        }
    }
}
