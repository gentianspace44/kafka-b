using VPS.Domain.Models.BluVoucher.Requests;
using VPS.Domain.Models.Common;
using VPS.Domain.Models.Enums;

namespace VPS.Services.BluVoucher
{
    public interface IBluVoucherKafkaProducer
    {
        Task Produce(BluVoucherRedeemRequest bluVoucherRedeemRequest, VoucherType voucherType, RedeemOutcome outcome);
    }
}
