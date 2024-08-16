using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VPS.Domain.Models.Configurations;
using Xunit;
using VPS.Provider.RACellularVoucher;
using VPS.Domain.Models.Configurations.RACellularVoucher;

namespace VPS.Tests.RACellularVoucher.ConfigExtentions;
public class RACellularVoucherConfigExtentionTests
{
    [Fact]
    public static void AddBluVoucherConfigurationSettings_ShouldAddServices()
    {
        // Arrange
        var services = new ServiceCollection();

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions() { });
        // Act
        services.AddRACellularConfigurationSettings(builder);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var raVoucherConfig = serviceProvider.GetService<IOptions<RACellularVoucherConfiguration>>();
        var kafkaQueueConfig = serviceProvider.GetService<IOptions<KafkaQueueConfiguration>>();

        Assert.NotNull(raVoucherConfig);
        Assert.NotNull(kafkaQueueConfig);
    }
}
