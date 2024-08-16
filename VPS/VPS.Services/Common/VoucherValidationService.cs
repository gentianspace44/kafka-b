using System.Reflection;
using VPS.Domain.Models.Common.Request;
using VPS.Domain.Models.Enums;
using VPS.Helpers.Logging;

namespace VPS.Services.Common
{
    public class VoucherValidationService : IVoucherValidationService
    {

        private readonly ILoggerAdapter<VoucherValidationService> _log;

        public VoucherValidationService(ILoggerAdapter<VoucherValidationService> log)
        {
            this._log = log ?? throw new System.ArgumentNullException(nameof(log));
        }

        public string IsVoucherRequestValid(VoucherRedeemRequestBase? voucherRedeemRequest)
        {
            var validationOutcome = "";

            if (voucherRedeemRequest == null)
            {
                _log.LogError(null, "Invalid request - voucher redeem request is null.");

                return "Invalid Request";
            }

            if (string.IsNullOrWhiteSpace(voucherRedeemRequest.VoucherNumber))
            {
                _log.LogError(null, "Missing voucher pin/number - {VoucherNumber}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    voucherRedeemRequest.VoucherNumber);

                validationOutcome = "Missing voucher pin/number";
            }
            else if (
                //try parse to make sure we have a valid long in terms of voucher pin/number.
                !long.TryParse(voucherRedeemRequest.VoucherNumber, out long voucherNum)
                || voucherNum <= 0
                || voucherRedeemRequest.VoucherNumber.Length < 10
                // Redundant: long cannot be bigger/longer than 9223372036854775808 anyway
                || voucherRedeemRequest.VoucherNumber.Length >= 30
                )
            {
                _log.LogError(null, "Invalid voucher pin entered. Pin: {VoucherNumber} for client ID {ClientId}.",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    voucherRedeemRequest.VoucherNumber, voucherRedeemRequest.ClientId);

                validationOutcome = "Invalid voucher pin entered";
            }
            else if (voucherRedeemRequest.ClientId <= 0)
            {
                _log.LogError(null, "Invalid Client ID - {ClientId} for pin {VoucherNumber}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                   voucherRedeemRequest.ClientId, voucherRedeemRequest.VoucherNumber);

                validationOutcome = "Invalid Client ID";
            }
            else if (string.IsNullOrEmpty(voucherRedeemRequest.DevicePlatform))
            {
                _log.LogError(null, "Missing Platform - {DevicePlatform} for pin {VoucherNumber}",
                    MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                    voucherRedeemRequest.DevicePlatform, voucherRedeemRequest.VoucherNumber);

                validationOutcome = "Missing Platform";
            }

            return validationOutcome;
        }

        public int GetVoucherNumberLength(VoucherType voucherType)
        {
            switch (voucherType)
            {
                case VoucherType.BluVoucher:
                    return 16;
                case VoucherType.EasyLoad:
                    return 14;
                case VoucherType.Flash:
                    return 16;
                case VoucherType.OTT:
                    return 12;
                case VoucherType.HollyTopUp:
                    return 15;
                case VoucherType.RACellular:
                    return 11;
                default:
                    throw new NotSupportedException($"Unsupported voucher type: {voucherType}");
            }
        }
    }
}
