using System.Xml.Serialization;

namespace VPS.Domain.Models.BluVoucher.Requests
{
    [XmlRoot(ElementName = "event")]
    public class AirtimeRequestEvent
    {
        [XmlElement(ElementName = "Reference")]
        public string? Reference { get; set; }
        [XmlElement(ElementName = "amount")]
        public string? Amount { get; set; }
        [XmlElement(ElementName = "PIN")]
        public string? Pin { get; set; }
        [XmlElement(ElementName = "VoucherPin")]
        public string? VoucherPin { get; set; }
    }
}
