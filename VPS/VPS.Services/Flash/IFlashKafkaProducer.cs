using VPS.Domain.Models.Common;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.Flash.Requests;

namespace VPS.Services.Flash
{
    public interface IFlashKafkaProducer
    {
        Task Produce(FlashRedeemRequest voucherRedeemRequest, VoucherType voucherType, RedeemOutcome outcome);
    }
}
