using VPS.Domain.Models.Common;
using VPS.Domain.Models.EasyLoad.Request;
using VPS.Domain.Models.Enums;

namespace VPS.Services.EasyLoad
{
    public interface IEasyLoadKafkaProducer
    {
        Task Produce(EasyLoadVoucherRedeemRequest voucherRedeemRequest, VoucherType voucherType, RedeemOutcome outcome);

    }
}
