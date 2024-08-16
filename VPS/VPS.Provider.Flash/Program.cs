using Prometheus;
using VPS.API.Common;
using VPS.API.Flash;
using VPS.Domain.Models.Configurations.Flash;
using VPS.Domain.Models.Enums;
using VPS.Provider.Flash;
using VPS.Services.Common;
using VPS.Services.CustomServiceExtentions;
using VPS.Services.Flash;
using VPS.Services.Kafka;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true);

// Add services to the container.
builder.Services.AddDefaultServicesForProviders(builder.Configuration);
builder.Services.AddDefaultKafkaService();
builder.Services.AddFlashConfigurationSettings(builder);
builder.AddCustomLogger(VpsConfiguration.Flash);
builder.Services.AddScoped<IClientBalanceService, ClientBalanceService<FlashRedeemService>>();
builder.Services.AddOptions();
builder.Services.Configure<FlashConfiguration>(builder.Configuration.GetSection("FlashConfiguration"));
builder.Services.AddScoped<IFlashKafkaProducer, FlashKafkaProducer>();
builder.Services.AddScoped<IVpsKafkaConsumer, FlashKafkaConsumer>();
builder.Services.AddScoped<IFlashApiAuthenticationService, FlashApiAuthenticationService>();
builder.Services.AddScoped<IFlashApiService, FlashApiService>();
builder.Services.AddScoped<IRedisServiceBridge, RedisServiceBridge>();
builder.Services.AddScoped<FlashRedeemService>();
await builder.Services.InitializeProviders();
var app = builder.Build();
app.UseMetricServer();
app.MapHealthChecks("/detailhealthcheck");
// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
// Use extension to process request
app.UseVoucherProcessor();
app.Run();
