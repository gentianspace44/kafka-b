using Prometheus;
using VPS.Domain.Models.BluVoucher;
using VPS.Domain.Models.Enums;
using VPS.Provider.BluVoucher;
using VPS.Services.BluVoucher;
using VPS.Services.BluVoucher.Mocks;
using VPS.Services.Common;
using VPS.Services.CustomServiceExtentions;
using VPS.Services.Kafka;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true);

// Add services to the container.
builder.Services.AddDefaultServicesForProviders(builder.Configuration);
builder.Services.AddDefaultKafkaService();
builder.Services.AddBluVoucherConfigurationSettings(builder);

builder.AddCustomLogger(VpsConfiguration.BluVoucher);
builder.Services.AddScoped<IClientBalanceService, ClientBalanceService<BluVoucher>>();

builder.Services.AddScoped<IBluVoucherKafkaProducer, BluVoucherKafkaProducer>();
builder.Services.AddScoped<IVpsKafkaConsumer, BluVoucherKafkaConsumer>();

bool injectMocks = Convert.ToBoolean(builder.Configuration["InjectMockServices"]?.ToString());

if (injectMocks)
{
    builder.Services.AddScoped<IRemitBluVoucherService, MockRemitBluVoucherService>();
}
else
{
    builder.Services.AddScoped<IRemitBluVoucherService, RemitBluVoucherService>();
}

builder.Services.AddScoped<BluVoucherRedeemService>();

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

