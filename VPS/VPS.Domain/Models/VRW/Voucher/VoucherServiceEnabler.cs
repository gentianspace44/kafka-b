using VPS.Domain.Models.Enums.VRWEnum;

namespace VPS.Domain.Models.VRW.Voucher
{
    public class VoucherServiceEnabler
    {

        public int VoucherProviderId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int VoucherTypeId { get; set; }

        public string ImageSource { get; set; } = string.Empty;

        public bool IsEnabled { get; set; }
        public bool IsVisible { get; set; }

        public string MicroServiceUrl { get; set; } = string.Empty;

        public string? SyxCreditServiceUrl { get; set; }

        public bool? UseSxyCreditEndPoint { get; set; }

        public string VoucherLength { get { return VoucherType.VoucherLength; } }

        public VoucherTypeModel VoucherType { get; set; } = new VoucherTypeModel();

        public bool InvertImage { get { return VoucherTypeId == (int)Provider.EasyLoad || VoucherTypeId == (int)Provider.OTT; } }
    }

    public class VoucherTypeModel
    {
        public int VoucherTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string VoucherLength { get; set; } = string.Empty;
    }
}
