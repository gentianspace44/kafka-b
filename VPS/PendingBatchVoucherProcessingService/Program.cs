using PendingBatchVoucherProcessingService;
using VPS.API.Common;
using VPS.API.Syx;
using VPS.Domain.Models.Configurations;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Infrastructure.Repository.Redis;
using VPS.Services.Common;


IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        config.AddJsonFile("appsettings.Development.json", optional: true);
    })
    .ConfigureServices((hostContext, services) =>
    {
        ConfigurationHelper.Initialize(hostContext.Configuration);
        services.AddTransient(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>));
        services.AddHttpClient();
        services.AddHttpContextAccessor();
        services.AddSingleton<IHttpClientWrapper, HttpClientWrapper>();
        services.AddSingleton<IHttpClientCommunication, HttpClientCommunication>();
        services.AddSingleton<IClientBonusService, ClientBonusService>();
        services.AddSingleton<IReferenceGeneratorService, ReferenceGeneratorService>();
        services.AddSingleton<IEligibleBonusRepository, EligibleBonusRepository>();
        services.AddSingleton<IVoucherLogRepository, VoucherLogRepository>();
        services.AddSingleton<IVoucherBatchProcessingRepository, VoucherBatchProcessingRepository>();
        services.AddSingleton<IBatchServiceRedisRepository, BatchServiceRedisRepository>();
        services.AddSingleton<IVoucherValidationService, VoucherValidationService>();
        services.AddSingleton<ExceptionMiddleware>();
        services.AddSingleton<MetricsHelper>();
        services.AddSingleton<BatchServiceRedisStore>();
        services.Configure<DbConnectionStrings>(hostContext.Configuration.GetSection("ConnectionStrings"));
        services.Configure<SyxSettings>(hostContext.Configuration.GetSection("SyxSettings"));
        services.Configure<VpsControlCenterEndpoints>(hostContext.Configuration.GetSection("VPSControlCenterEndpoints"));
        services.Configure<PendingBatchVoucherSettings>(hostContext.Configuration.GetSection("PendingBatchVoucherSettings"));
        services.Configure<VoucherRedeemClientNotifications>(hostContext.Configuration.GetSection("VoucherRedeemClientNotifications"));
        services.Configure<BatchProcessingDBSettings>(hostContext.Configuration.GetSection("BatchProcessingDBSettings"));
        services.Configure<BatchServiceRedisSettings>(hostContext.Configuration.GetSection("BatchServiceRedisSettings"));
        services.Configure<CountrySettings>(hostContext.Configuration.GetSection("CountrySettings"));

        services.AddSingleton<IClientBalanceService, ClientBalanceService<Worker>>();
        services.AddSingleton<ISyxApiService, SyxApiService>();

        services.AddHostedService<Worker>();
        services.Configure<HostOptions>(options =>
        {
            options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
        });
        services.BuildServiceProvider();
    })
    .UseWindowsService()
    .Build();

await host.RunAsync();