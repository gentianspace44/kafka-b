using Prometheus;
using VPS.API.RACellularVoucher;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.RACellularVoucher;
using VPS.Provider.RACellularVoucher;
using VPS.Services.Common;
using VPS.Services.CustomServiceExtentions;
using VPS.Services.Kafka;
using VPS.Services.RACellularVoucher;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true);

// Add services to the container.
builder.Services.AddDefaultServicesForProviders(builder.Configuration);
builder.Services.AddDefaultKafkaService();
builder.Services.AddRACellularConfigurationSettings(builder);

builder.AddCustomLogger(VpsConfiguration.RACellular);
builder.Services.AddScoped<IClientBalanceService, ClientBalanceService<RaCellularVoucher>>();
builder.Services.AddScoped<IRaCellularVoucherKafkaProducer, RaCellularVoucherKafkaProducer>();
builder.Services.AddScoped<IVpsKafkaConsumer, RaCellularVoucherKafkaConsumer>();

builder.Services.AddScoped<IraCellularVoucherApiService, RaCellularVoucherApiService>();
builder.Services.AddScoped<RaCellularVoucherRedeemService>();

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
