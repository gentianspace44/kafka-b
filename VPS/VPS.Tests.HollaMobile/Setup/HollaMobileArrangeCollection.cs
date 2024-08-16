using VPS.Domain.Models.Common;
using VPS.Domain.Models.HollaMobile.Requests;
using VPS.Domain.Models.HollaMobile.Response;


namespace VPS.Tests.HollaMobile.Setup
{
    public static class HollaMobileArrangeCollection
    {
        public static HollaMobileRedeemResponse CreateSuccessHollaMobileVoucherResponse()
        {
            var random = new Random();

            return new HollaMobileRedeemResponse
            {
                VoucherAmount = 1.00M,
                CreateDatetime = DateTime.Now,
                StatusID = 1,
                VoucherID = random.Next(1, 7)
            };
        }

        public static RedeemOutcome SuccessRedeemOutCome(decimal voucherAmount, long voucherId)
        {
            return new RedeemOutcome
            {
                OutComeTypeId = 1,
                OutcomeMessage = "Airtime redeem successfully",
                VoucherAmount = voucherAmount,
                VoucherID = voucherId
            };
        }

        public static HollaMobileRedeemRequest CreateVoucherRedeemRequest()
        {
            return new HollaMobileRedeemRequest()
            {
                ClientId = 10009107,
                DevicePlatform = "newweb",
                VoucherNumber = "0459507223232265",
                VoucherReference = "27834621771y63"
            };
        }

        public static HollaMobileRedeemResponse CreateErrorVoucherResponse()
        {
            var random = new Random();

            return new HollaMobileRedeemResponse
            {
                VoucherID = random.Next(1, 7),
                VoucherAmount = 1.00M,
                CreateDatetime = DateTime.Now,
                StatusID = 3
            };
        }

        public static RedeemOutcome FailVoucherRedeemOutCome(decimal voucherAmount, int voucherID)
        {
            return new RedeemOutcome
            {
                OutcomeMessage = "Invalid airtime",
                OutComeTypeId = -1,
                VoucherAmount = voucherAmount,
                VoucherID = voucherID
            };
        }

        public static RedeemOutcome AlreadyRedeemedOutCome()
          => new()
          {
              OutcomeMessage = "Airtime already redeemed",
              OutComeTypeId = -1,
          };

        public static HollaMobileRedeemResponse CreateAlreadyRedeemedHollaMobileVoucherResponse()
            => new()
            {
                StatusID = 2
            };
    }
}
