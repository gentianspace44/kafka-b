using FluentValidation;
using Microsoft.AspNetCore.ResponseCompression;
using Prometheus;
using VPS.API.Common;
using VPS.API.CreditingService;
using VPS.Domain.Models.Configurations;
using VPS.Domain.Models.Configurations.VRWConfiguration;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.VRW.Voucher;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.RedemptionWidget.SignalR;
using VPS.Services.Common;
using VPS.Services.CustomServiceExtentions;
using VPS.Services.VRW.Interface;
using VPS.Services.VRW.Services;

var builder = WebApplication.CreateBuilder(args);

ConfigurationHelper.Initialize(builder.Configuration);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true);

builder.AddCustomLogger(VpsConfiguration.VRW);
builder.Services.AddScoped<IValidator<VrwViewModel>, VoucherRedemptionModelValidator>();
builder.Services.AddSignalR();
builder.Services.AddScoped<IVoucherRedemptionWidgetService, VoucherRedemptionWidgetService>();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
});

builder.Services.Configure<VpsControlCenterEndpoints>(builder.Configuration.GetSection("VPSControlCenterEndpoints"));
builder.Services.Configure<SiteConfiguration>(builder.Configuration.GetSection("SiteConfiguration"));
builder.Services.Configure<CountrySettings>(builder.Configuration.GetSection("CountrySettings"));
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddMemoryCache();

// Register your services with their respective interfaces or base classes.
builder.Services.AddSingleton<MetricsHelper>();
builder.Services.AddTransient(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>));
builder.Services.AddHttpClient();
builder.Services.AddTransient<IHttpClientWrapper, HttpClientWrapper>();
builder.Services.AddScoped<IHttpClientCommunication, HttpClientCommunication>();
builder.Services.AddTransient<ExceptionMiddleware>();
builder.Services.AddScoped<CreditingService>();
builder.Services.AddScoped<ICreditingService, VpsCrediting>();
await builder.Services.InitializeProviders();

var app = builder.Build();
app.UseMetricServer();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();

var whiteListedUrls = builder.Configuration.GetSection("SiteConfiguration:WhiteListedUrls").Value;

app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.Add("Content-Security-Policy",
        $"frame-ancestors 'self' {whiteListedUrls}");
    await next();
});

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
     endpoints.MapControllers();
   endpoints.MapHub<ClearCacheHub>("/clearCacheHub");

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

// Use extension to process VRW RedemptionWidget request

app.Run();
