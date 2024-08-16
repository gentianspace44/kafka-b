using VPS.Domain.Models.Common.Request;
using VPS.Domain.Models.Enums;

namespace VPS.Services.Common
{
    public interface IVoucherValidationService
    {
        string IsVoucherRequestValid(VoucherRedeemRequestBase? voucherRedeemRequest);
        public int GetVoucherNumberLength(VoucherType voucherType);
    }
}
