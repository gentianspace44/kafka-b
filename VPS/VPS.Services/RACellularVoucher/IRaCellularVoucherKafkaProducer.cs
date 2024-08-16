using VPS.Domain.Models.Common;
using VPS.Domain.Models.Enums;
using VPS.Domain.Models.RACellularVoucher.Requests;

namespace VPS.Services.RACellularVoucher;

public interface IRaCellularVoucherKafkaProducer
{
    Task ProduceAsync(RaCellularVoucherRedeemRequest raCellularVoucherRedeemRequest, VoucherType voucherType, RedeemOutcome outcome);
}
