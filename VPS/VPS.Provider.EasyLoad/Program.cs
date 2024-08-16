using Prometheus;
using VPS.API.EasyLoad;
using VPS.API.EasyLoad.Mocks;
using VPS.Domain.Models.EasyLoad;
using VPS.Domain.Models.Enums;
using VPS.Helpers;
using VPS.Provider.EasyLoad;
using VPS.Services.Common;
using VPS.Services.CustomServiceExtentions;
using VPS.Services.EasyLoad;
using VPS.Services.Kafka;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true);

// Add services to the container.
builder.Services.AddDefaultServicesForProviders(builder.Configuration);
builder.Services.AddDefaultKafkaService();
builder.Services.AddEasyLoadConfigurationSettings(builder);

builder.AddCustomLogger(VpsConfiguration.EasyLoad);
builder.Services.AddScoped<IClientBalanceService, ClientBalanceService<EasyLoadVoucher>>();
bool injectMocks = Convert.ToBoolean(builder.Configuration["InjectMockServices"]?.ToString());

if (injectMocks)
{
    builder.Services.AddScoped<IEasyLoadApiService, MockEasyLoadApiService>();
}
else
{
    builder.Services.AddScoped<IEasyLoadApiService, EasyLoadApiService>();
}

builder.Services.AddScoped<IEasyLoadKafkaProducer, EasyLoadKafkaProducer>();
builder.Services.AddScoped<IVpsKafkaConsumer, EasyLoadKafkaConsumer>();
builder.Services.AddScoped<EasyLoadVoucherRedeemService>();

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


