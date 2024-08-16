using VPS.Domain.Models.BluVoucher.Responses;

namespace VPS.Test.BluVoucher.Models.BluVoucher.Responses
{
    public class BluVoucherProviderXmlResponseTests
    {
        private BluVoucherProviderXmlResponse _bluVoucherProviderXmlResponseMock;

        public BluVoucherProviderXmlResponseTests()
        {
            _bluVoucherProviderXmlResponseMock = new BluVoucherProviderXmlResponse();
        }

        [Fact]
        public void CanSetAndGetSessionId()
        {
            // Arrange
            var testValue = "TestValue321584811";

            // Act
            _bluVoucherProviderXmlResponseMock.SessionId = testValue;

            // Assert
            Assert.Equal(testValue, _bluVoucherProviderXmlResponseMock.SessionId);
        }

        [Fact]
        public void CanSetAndGetEventType()
        {
            // Arrange
            var testValue = "TestValue160906758";

            // Act
            _bluVoucherProviderXmlResponseMock.EventType = testValue;

            // Assert
            Assert.Equal(testValue, _bluVoucherProviderXmlResponseMock.EventType);
        }

        [Fact]
        public void CanSetAndGetEvent()
        {
            // Arrange
            var testValue = new EventRedeemStatusCode { EventCode = "TestValue695821730" };

            // Act
            _bluVoucherProviderXmlResponseMock.Event = testValue;

            // Assert
            Assert.Same(testValue, _bluVoucherProviderXmlResponseMock.Event);
        }

        [Fact]
        public void CanSetAndGetData()
        {
            // Arrange
            var testValue = new RedeemData
            {
                Status = new RedeemEventDetails
                {
                    Code = "TestValue1619685330",
                    Description = "TestValue1069426439",
                    Amount = 1216832937.21M,
                    RedemtionTransRef = "TestValue1567158734",
                    RedemtionDate = "TestValue1371585244"
                },
                Reference = "TestValue2139148201"
            };

            // Act
            _bluVoucherProviderXmlResponseMock.Data = testValue;

            // Assert
            Assert.Same(testValue, _bluVoucherProviderXmlResponseMock.Data);
        }
    }
}