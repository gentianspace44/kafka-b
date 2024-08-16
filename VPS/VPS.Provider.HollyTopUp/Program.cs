using Prometheus;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.HollyTopUp;
using VPS.Infrastructure.Repository.HollyTopUp;
using VPS.Provider.HollyTopUp;
using VPS.Services.Common;
using VPS.Services.CustomServiceExtentions;
using VPS.Services.HollyTopUp;
using VPS.Services.Kafka;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true);

// Add services to the container.
builder.Services.AddDefaultServicesForProviders(builder.Configuration);
builder.Services.AddDefaultKafkaService();
builder.Services.AddHollyTopUpConfigExtention(builder);


builder.AddCustomLogger(VpsConfiguration.HTU);
builder.Services.AddScoped<IClientBalanceService, ClientBalanceService<HollyTopUpVoucher>>();
builder.Services.AddScoped<IHollyTopUpRepository, HollyTopUpRepository>();
builder.Services.AddScoped<HollyTopUpRedeemService>();
builder.Services.AddScoped<IHollyTopUpKafkaProducer, HollyTopUpKafkaProducer>();
builder.Services.AddScoped<IVpsKafkaConsumer, HollyTopUpKafkaConsumer>();


await builder.Services.InitializeProviders();

var app = builder.Build();
app.UseMetricServer();
app.MapHealthChecks("/detailhealthcheck");
//Configure the HTTP request pipeline
app.UseHttpsRedirection();

// Use extension to process request
app.UseVoucherProcessor();
app.Run();
