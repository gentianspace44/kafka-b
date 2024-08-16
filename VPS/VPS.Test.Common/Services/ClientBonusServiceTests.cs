using NSubstitute;
using NSubstitute.ExceptionExtensions;
using VPS.Domain.Models.Common;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Services.Common;
using VPS.Test.Common.Setup;

namespace VPS.Test.Common.Services
{
    public class ClientBonusServiceTests
    {
        private readonly ILoggerAdapter<ClientBonusService> _log = Substitute.For<ILoggerAdapter<ClientBonusService>>();
        private readonly IEligibleBonusRepository _eligibleBonusRepository = Substitute.For<IEligibleBonusRepository>();
        private readonly ClientBonusService _clientBonusService;
        private readonly EligibleVoucherBonus _activeBonus;
        public ClientBonusServiceTests()
        {
            _activeBonus = Fixtures.SetActiveBonus();
            _clientBonusService = new ClientBonusService(_log, _eligibleBonusRepository);
        }
        [Fact]
        public async Task GetBonusAmount_NoPreviousBonus_ReturnsIncomingBonusAmount()
        {
            // Arrange
            int clientId = 1;
            decimal redeemAmount = 50;

            // Mock the eligibleBonusService.GetPunterBonusTotals method to return null (no previous bonus).
            _eligibleBonusRepository.GetPunterBonusTotals(_activeBonus.BonusID, clientId).Returns(Task.FromResult<PunterBonusTransaction?>(null));

            // Act
            var result = await _clientBonusService.GetBonusAmount(_activeBonus, redeemAmount, clientId);

            // Assert
            Assert.Equal(5, result); // 10% of 50
        }


        [Fact]
        public async Task GetBonusAmount_BonusExceedsMaxRedeemAmount_ReturnsMaxBonusAmount()
        {
            // Arrange
            int clientId = 1;
            decimal redeemAmount = 150; // Exceeds max redeem amount

            // Mock the eligibleBonusService.GetPunterBonusTotals method to return null (no previous bonus).
            _eligibleBonusRepository.GetPunterBonusTotals(_activeBonus.BonusID, clientId).Returns(Task.FromResult<PunterBonusTransaction?>(null));

            // Act
            var result = await _clientBonusService.GetBonusAmount(_activeBonus, redeemAmount, clientId);

            // Assert
            Assert.Equal(10, result); // Should be capped at max bonus amount (10% of 100)
        }

        [Fact]
        public async Task GetBonusAmount_SpecialCase_MaxBonusInOneGo()
        {
            // Arrange
            int clientId = 1;
            decimal redeemAmount = 200; // Exceeds max redeem amount

            // Mock the eligibleBonusService.GetPunterBonusTotals method to return null (no previous bonus).
            _eligibleBonusRepository.GetPunterBonusTotals(_activeBonus.BonusID, clientId).Returns(Task.FromResult<PunterBonusTransaction?>(null));

            // Act
            var result = await _clientBonusService.GetBonusAmount(_activeBonus, redeemAmount, clientId);

            // Assert
            Assert.Equal(10, result); // Should be capped at max bonus amount (10% of 100)
        }

        [Fact]
        public async Task GetBonusAmount_ErrorOccurs_ReturnsZero()
        {
            // Arrange
            int clientId = 1;
            decimal redeemAmount = 50;

            // Mock the eligibleBonusService.GetPunterBonusTotals method to throw an exception.
            _eligibleBonusRepository.GetPunterBonusTotals(_activeBonus.BonusID, clientId).Throws(new Exception("Simulated error"));

            // Act
            var result = await _clientBonusService.GetBonusAmount(_activeBonus, redeemAmount, clientId);

            // Assert
            Assert.Equal(0, result); // Error occurred, should return 0
        }
    }
}
