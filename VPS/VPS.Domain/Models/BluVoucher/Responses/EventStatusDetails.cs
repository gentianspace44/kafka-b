using System.Xml.Serialization;

namespace VPS.Domain.Models.BluVoucher.Responses
{
    [XmlRoot(ElementName = "Status")]
    public class EventStatusDetails
    {
        [XmlElement(ElementName = "Code")]
        public string? Code { get; set; }
        [XmlElement(ElementName = "Description")]
        public string? Description { get; set; }
        [XmlElement(ElementName = "Amount")]
        public string? Amount { get; set; }
        [XmlElement(ElementName = "DateRedeemed")]
        public string? DateRedeemed { get; set; }
        [XmlElement(ElementName = "RedeemedReference")]
        public string? RedeemedReference { get; set; }
    }
}
