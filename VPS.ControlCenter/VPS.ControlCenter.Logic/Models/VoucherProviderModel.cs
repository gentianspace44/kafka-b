using System.ComponentModel.DataAnnotations;

namespace VPS.ControlCenter.Logic.Models
{
    public class VoucherProviderModel
    {
        public int VoucherProviderId { get; set; }
        [MaxLength(250)]
        public string Name { get; set; }
        public int VoucherTypeId { get; set; }
        [MaxLength(1000)]
        public string ImageSource { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsVisible { get; set; }
        [MaxLength(1000)]
        public string MicroServiceUrl { get; set; }

        [MaxLength(1000)]
        public string? SyxCreditServiceUrl { get; set; }

        public bool? UseSxyCreditEndPoint { get; set; }

        public VoucherTypeModel VoucherType { get; set; }
    }

    public class VoucherProviderList
    {
        public List<VoucherProviderModel> VoucherProviders { get; set; }
    }
}
