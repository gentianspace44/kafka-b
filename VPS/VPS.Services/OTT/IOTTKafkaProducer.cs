using VPS.Domain.Models.Common;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.OTT.Requests;

namespace VPS.Services.OTT
{
    public interface IOttKafkaProducer
    {
        Task Produce(OttVoucherRedeemRequest oTTVoucherRedeemRequest, VoucherType voucherType, RedeemOutcome outcome);
    }
}
