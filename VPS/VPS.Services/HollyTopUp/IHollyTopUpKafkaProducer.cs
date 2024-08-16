using VPS.Domain.Models.Common;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.HollyTopUp.Requests;

namespace VPS.Services.HollyTopUp
{
    public interface IHollyTopUpKafkaProducer
    {
        Task Produce(HollyTopUpRedeemRequest hollyTopUpRedeemRequest, VoucherType voucherType, RedeemOutcome outcome);
    }
}
