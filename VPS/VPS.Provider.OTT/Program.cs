using Prometheus;
using VPS.API.OTT;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.OTT;
using VPS.Provider.OTT;
using VPS.Services.Common;
using VPS.Services.CustomServiceExtentions;
using VPS.Services.Kafka;
using VPS.Services.OTT;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true);

// Add services to the container.
builder.Services.AddDefaultServicesForProviders(builder.Configuration);
builder.Services.AddDefaultKafkaService();
builder.Services.AddOTTVoucherConfigurationSettings(builder);

builder.AddCustomLogger(VpsConfiguration.OTT);
builder.Services.AddScoped<IClientBalanceService, ClientBalanceService<OttVouchers>>();
builder.Services.AddScoped<OttVoucherRedeemService>();
builder.Services.AddScoped<IOttKafkaProducer, OttKafkaProducer>();
builder.Services.AddScoped<IVpsKafkaConsumer, OttKafkaConsumer>();
builder.Services.AddScoped<IOttApiService, OttApiService>();

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
