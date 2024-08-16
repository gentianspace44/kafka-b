using Microsoft.Extensions.Options;
using NSubstitute;
using VPS.Domain.Models.Configurations;
using VPS.Helpers;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.HollaMobile;
using VPS.Infrastructure.Repository.HollyTopUp;
using Xunit;

namespace VPS.Tests.HollaMobile.Repositories
{
    public class HollaMobileRepositoryTests
    {
        private readonly MetricsHelper _metricsHelper = Substitute.For<MetricsHelper>();
        [Fact]
        public void Construct_GivenILoggerAdapterIsNull_ShouldThrowException()
        {
            
            //Arrange
            //Act
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                return new HollaMobileRepository(Substitute.For<IOptions<DbConnectionStrings>>(),
                                                null!, _metricsHelper);
            });

            //Assert
            Assert.Equal("log", exception.ParamName);


        }

        [Fact]
        public void Construct_GivenDbConnectionStringsIsNull_ShouldThrowException()
        {
            //Arrange
            //Act
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                return new HollaMobileRepository(null!,
                                                Substitute.For<ILoggerAdapter<HollaMobileRepository>>(), _metricsHelper);
            });

            //Assert
            Assert.Equal("dbConnectionStrings", exception.ParamName);
        }
    }
}
