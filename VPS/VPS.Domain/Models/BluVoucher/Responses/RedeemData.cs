using System.Xml.Serialization;

namespace VPS.Domain.Models.BluVoucher.Responses
{
    [XmlRoot(ElementName = "data")]
    public class RedeemData
    {
        [XmlElement(ElementName = "Status")]
        public RedeemEventDetails? Status { get; set; }
        [XmlElement(ElementName = "Reference")]
        public string? Reference { get; set; }
    }
}
