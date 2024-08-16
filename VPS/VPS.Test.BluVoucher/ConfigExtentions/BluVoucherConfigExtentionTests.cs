using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VPS.Domain.Models.Configurations.BluVoucher;
using VPS.Domain.Models.Configurations;
using VPS.Services.BluVoucher;
using VPS.Provider.BluVoucher;

namespace VPS.Test.BluVoucher.ConfigExtentions;
public class BluVoucherConfigExtentionTests
{
    [Fact]
    public static void AddBluVoucherConfigurationSettings_ShouldAddServices()
    {
        // Arrange
        var services = new ServiceCollection();

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions() { });
        builder.Services.AddSingleton<IAirtimeAuthentication, AirtimeAuthentication>();

        // Act
        services.AddBluVoucherConfigurationSettings(builder);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var bluVoucherConfig = serviceProvider.GetService<IOptions<BluVoucherConfiguration>>();
        var kafkaQueueConfig = serviceProvider.GetService<IOptions<KafkaQueueConfiguration>>();

        Assert.NotNull(bluVoucherConfig);
        Assert.NotNull(kafkaQueueConfig);
    }
}