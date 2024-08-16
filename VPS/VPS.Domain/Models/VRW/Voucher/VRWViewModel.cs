using System.ComponentModel.DataAnnotations;

namespace VPS.Domain.Models.VRW.Voucher
{
    public class VrwViewModel
    {
        [Display(Name = "I don't know what my voucher provider type is")]
        public bool DefaultVoucherProvider { get; set; }

        public int HasVoucherProvider { get; set; }

        public IEnumerable<VoucherServiceEnabler> VouchersEnablers { get; set; } = new List<VoucherServiceEnabler>();

        public string Message { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public string ClientId { get; set; } = string.Empty;

        public string DevicePlatform { get; set; } = string.Empty;

        public int ToggleHelpText { get; set; }

        public bool UseSyxCredit { get; set; }
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public string VoucherNumber { get; set; } = string.Empty;

        public string VoucherName { get; set; } = string.Empty;

        public string SyxVoucherCreditingEndPoint { get; set; } = string.Empty;
        public string VoucherNumberLength { get; set; } = string.Empty;

        public string SignalRConnectionId { get; set; } = string.Empty;
        public bool IsSuccessful { get; set; }
    }
}
