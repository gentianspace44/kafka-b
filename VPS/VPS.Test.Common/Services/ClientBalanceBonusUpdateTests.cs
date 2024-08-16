using NSubstitute;
using VPS.API.Syx;
using VPS.Domain.Models.Enums;
using VPS.Helpers.Logging;
using VPS.Infrastructure.Repository.Common;
using VPS.Services.Common;
using VPS.Test.Common.Models;
using VPS.Test.Common.Setup;

namespace VPS.Test.Common.Services
{
    public class ClientBalanceBonusUpdateTests
    {
        private readonly ILoggerAdapter<ClientBalanceService<ProviderVoucher>> _log = Substitute.For<ILoggerAdapter<ClientBalanceService<ProviderVoucher>>>();
        
        private readonly ISyxApiService _syxService = Substitute.For<ISyxApiService>();
        private readonly IClientBonusService _clientBonusService = Substitute.For<IClientBonusService>();
        private readonly IEligibleBonusRepository _eligibleBonusRepository = Substitute.For<IEligibleBonusRepository>();
        private readonly IReferenceGeneratorService _referenceGenerator = Substitute.For<IReferenceGeneratorService>();
        private readonly IVoucherLogRepository _voucherLogRepository = Substitute.For<IVoucherLogRepository>();
        private readonly ClientBalanceService<ProviderVoucher> _clientBalanceBonusUpdate;

        public ClientBalanceBonusUpdateTests()
        {
            _clientBalanceBonusUpdate = new ClientBalanceService<ProviderVoucher>(_log, _syxService, _clientBonusService, _eligibleBonusRepository, _referenceGenerator, _voucherLogRepository);
        }

        [Fact]
        public async Task CreditOnSyX_Success()
        {
            //Arrange
            var credOnSyxRequest = ArrangeCollection.CreateCreditOnSyxRequest();

            _syxService.HealthCheck().ReturnsForAnyArgs(true);

            var voucherExistsResponse = ArrangeCollection.SuccessApiVoucherExistsResponseNewVoucher();
            _syxService.CheckVoucherExists(default, Arg.Any<string>()).ReturnsForAnyArgs(voucherExistsResponse);

            var voucherReference = ArrangeCollection.GetReference(credOnSyxRequest.Platform, credOnSyxRequest.VoucherPin);
            _referenceGenerator.Generate(VoucherType.EasyLoad, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(voucherReference);

            var sucessClientBalanceResponse = ArrangeCollection.SuccessApiClientBalanceUpdateResponse();
            _syxService.UpdateClientBalance(default, default, default, default, Arg.Any<string>()).ReturnsForAnyArgs(sucessClientBalanceResponse);

            var noEligibleBonusFound = ArrangeCollection.NoEligibleVoucherBonusFound();
            _eligibleBonusRepository.GetEligibleVoucherBonus(default, default).ReturnsForAnyArgs(noEligibleBonusFound);

            var expectedResponse = ArrangeCollection.SuccessSyxCreditOutCome();

            //Act
            var result = await _clientBalanceBonusUpdate.CreditOnSyX(credOnSyxRequest.ClientId, credOnSyxRequest.Platform, credOnSyxRequest.VoucherId, credOnSyxRequest.VoucherPin, credOnSyxRequest.Amount, credOnSyxRequest.VoucherPrefix, credOnSyxRequest.VoucherType, credOnSyxRequest.VoucherReference, string.Empty);

            //Assert
            Assert.True(expectedResponse.OutComeTypeId == result.OutComeTypeId);
        }

        [Fact]
        public async Task CreditOnSyX_Fail()
        {
            //Arrange
            var credOnSyxRequest = ArrangeCollection.CreateCreditOnSyxRequest();

            var voucherReference = ArrangeCollection.GetReference(credOnSyxRequest.Platform, credOnSyxRequest.VoucherPin);
            _referenceGenerator.Generate(VoucherType.EasyLoad, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(voucherReference);

            var failClientBalanceResponse = ArrangeCollection.FailApiClientBalanceUpdateResponse();
            _syxService.UpdateClientBalance(default, default, default, default, Arg.Any<string>()).ReturnsForAnyArgs(failClientBalanceResponse);

            var noEligibleBonusFound = ArrangeCollection.NoEligibleVoucherBonusFound();
            _eligibleBonusRepository.GetEligibleVoucherBonus(default, default).ReturnsForAnyArgs(noEligibleBonusFound);

            var expectedResponse = ArrangeCollection.FailSyxCreditOutCome();

            //Act
            var result = await _clientBalanceBonusUpdate.CreditOnSyX(credOnSyxRequest.ClientId, credOnSyxRequest.Platform, credOnSyxRequest.VoucherId, credOnSyxRequest.VoucherPin, credOnSyxRequest.Amount, credOnSyxRequest.VoucherPrefix, credOnSyxRequest.VoucherType, credOnSyxRequest.VoucherReference, string.Empty);

            //Assert
            Assert.True(expectedResponse.OutComeTypeId == result.OutComeTypeId);
        }

        [Fact]
        public async Task CreditBonusOnSyX_Success()
        {
            //Arrange
            var credBonusOnSyxRequest = ArrangeCollection.CreateCreditBonusOnSyXRequest();
            _clientBonusService.GetBonusAmount(default, default, default).ReturnsForAnyArgs(10);

            var sucessClientBalanceResponse = ArrangeCollection.SuccessApiClientBalanceUpdateResponse();
            _syxService.UpdateClientBalance(default, default, default, default, Arg.Any<string>()).ReturnsForAnyArgs(sucessClientBalanceResponse);

            var expectedResponse = ArrangeCollection.SuccessSyxCreditOutCome();

            //Act
            var result = await _clientBalanceBonusUpdate.CreditBonusOnSyX(credBonusOnSyxRequest.ClientId, credBonusOnSyxRequest.Platform, credBonusOnSyxRequest.VoucherId, credBonusOnSyxRequest.VoucherAmount, credBonusOnSyxRequest.EligibleVoucherBonus, credBonusOnSyxRequest.VoucherPrefix);

            //Assert
            Assert.True(expectedResponse.OutComeTypeId == result.OutComeTypeId);

        }

        [Fact]
        public async Task CreditBonusOnSyX_Fail()
        {
            //Arrange
            var credBonusOnSyxRequest = ArrangeCollection.CreateCreditBonusOnSyXRequest();
            _clientBonusService.GetBonusAmount(default, default, default).ReturnsForAnyArgs(10);

            var failClientBalanceResponse = ArrangeCollection.FailApiClientBalanceUpdateResponse();
            _syxService.UpdateClientBalance(default, default, default, default, Arg.Any<string>()).ReturnsForAnyArgs(failClientBalanceResponse);

            var expectedResponse = ArrangeCollection.FailSyxCreditOutCome();

            //Act
            var result = await _clientBalanceBonusUpdate.CreditBonusOnSyX(credBonusOnSyxRequest.ClientId, credBonusOnSyxRequest.Platform, credBonusOnSyxRequest.VoucherId, credBonusOnSyxRequest.VoucherAmount, credBonusOnSyxRequest.EligibleVoucherBonus, credBonusOnSyxRequest.VoucherPrefix);

            //Assert
            Assert.True(expectedResponse.OutComeTypeId == result.OutComeTypeId);

        }
    }
}
