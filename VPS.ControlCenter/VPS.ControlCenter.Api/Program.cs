using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Sinks.Elasticsearch;
using StackExchange.Redis;
using System.Text;
using VPS.ControlCenter.Api.Extensions;
using VPS.ControlCenter.Api.Hubs;
using VPS.ControlCenter.Core;
using VPS.ControlCenter.Core.HollyTopUp;
using VPS.ControlCenter.Logic.EFServices;
using VPS.ControlCenter.Logic.Helpers;
using VPS.ControlCenter.Logic.IServices;
using VPS.ControlCenter.Logic.Models;
using VPS.ControlCenter.Logic.OtherServices;
using VPS.ControlCenter.Logic.RedisServices;
using VPS.ControlCenter.Logic.SignalrServices;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true);
ConfigurationHelper.Initialize(builder.Configuration);
// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddDbContext<VpsDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("VpsDbContext"));
});

builder.Services.AddDbContext<HollyTopUpEntities>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("HollyTopUpEntities")));
builder.Services.AddControllers().AddNewtonsoftJson();


builder.Services.AddSignalR().AddStackExchangeRedis(options =>
{
    
    var redisConfig = builder.Configuration.GetSection("RedisSettings").Get<RedisSettings>();

    options.ConnectionFactory = async writer =>
    {
        var logger = builder.Services.BuildServiceProvider().GetService<ILogger<Program>>();
        var config = new ConfigurationOptions
        {
            AbortOnConnectFail = redisConfig != null && redisConfig.AbortOnConnectFail ,
            ChannelPrefix = new RedisChannel(redisConfig.ChannelPrefix, RedisChannel.PatternMode.Literal),
            ClientName = redisConfig.ClientName,
            ConnectRetry = redisConfig.ConnectRetry,
             DefaultDatabase = redisConfig.RedisDb                   
        };

        config.EndPoints.Add(redisConfig.RedisServer);

        var connection = await ConnectionMultiplexer.ConnectAsync(config, writer);

        connection.ConnectionFailed += (_, e) =>
        {
            logger?.LogError(e.Exception, "Connection to Redis failed.");
        };

        if (!connection.IsConnected)
        {
            logger?.LogError("Did not connect to Redis.");
        }

        return connection;
    };
});


var settings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddTransient<IVoucherProviderService, EFVoucherProviderService>();
builder.Services.AddSingleton<RedisStore>();
builder.Services.AddScoped<IRedisRepository, RedisRepository>();
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("RedisSettings"));
builder.Services.Configure<SyxSettings>(builder.Configuration.GetSection("SyxSettings"));
builder.Services.AddScoped<SignalRHelperService, SignalRHelperService>();
builder.Services.AddScoped<IAlertService, AlertServices>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        corsBuilder =>
        {
            var origins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>();
            corsBuilder.WithOrigins(origins)
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                                 .AllowCredentials();
        });
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(jwt =>
{
    var key = Encoding.ASCII.GetBytes(settings?.SecretKey);
    jwt.SaveToken = true;
    jwt.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        RequireExpirationTime = false
    };
});
Log.Logger = new LoggerConfiguration()
.MinimumLevel.Information()
.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
.MinimumLevel.Override("System", LogEventLevel.Warning)
.WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(builder.Configuration.GetSection("ElasticConfig:NodeUri").Value))
{
    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
    ModifyConnectionSettings = x => x.BasicAuthentication(builder.Configuration.GetSection("ElasticConfig:ElasticUserName").Value, builder.Configuration.GetSection("ElasticConfig:ElasticPassword").Value),
    IndexFormat = builder.Configuration.GetSection("ElasticConfig:IndexFormat").Value
})
.CreateLogger();


builder.Services.AddSingleton<ILoggerFactory>(new SerilogLoggerFactory(Log.Logger));
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);

var serviceProvider = builder.Services.BuildServiceProvider();
var yourService = serviceProvider.GetRequiredService<IVoucherProviderService>();
await yourService.SetOrUpdateRedis();


var app = builder.Build();

SyxSessionHelper.Initialize(app.Services, app.Configuration, serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<SyxSessionHelper>());
await SyxSessionHelper.ApiLogin();
SyxSessionHelper.StartKeepAliveTimer();
// Configure the HTTP request pipeline.


app.UseRouting();
app.UseHttpsRedirection();
app.UseCors("AllowAllOrigins");
app.UseAuthorization();
app.ConfigureEndpoints(serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<NotificationsHub>());

app.MapHub<DynamicSettingsHub>("/dynamicSettingsHub");
app.MapHub<NotificationsHub>("/notificationsHub");
app.MapHub<FeatureToggleHub>("/featureToggleHub");

app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();

app.Use(async (context, next) =>
{
    await next(context);
});


app.Run();

