using Prometheus;
using VPS.Domain.Models.Enums;
using VPS.Infrastructure.Repository.HollaMobile;
using VPS.Provider.HollaMobile;
using VPS.Services.Common;
using VPS.Services.CustomServiceExtentions;
using VPS.Services.HollaMobile;

var builder = WebApplication.CreateBuilder(args);


builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile("appsettings.development.json", optional: true);

// Add services to the container.
builder.Services.AddDefaultServicesForProviders(builder.Configuration);
builder.Services.AddHollaMobileConfigExtention(builder);


builder.AddCustomLogger(VpsConfiguration.HollaMobile);
builder.Services.AddScoped<IHollaMobileRepository, HollaMobileRepository>();
builder.Services.AddScoped<HollaMobileRedeemService>();


await builder.Services.InitializeProviders();

var app = builder.Build();
app.UseMetricServer();
app.MapHealthChecks("/detailhealthcheck");
//Configure the HTTP request pipeline
app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
// Use extension to process request
app.UseAirtimeProcessor();
app.Run();
