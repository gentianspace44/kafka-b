using System.Xml.Serialization;

namespace VPS.Domain.Models.BluVoucher.Responses
{
    [XmlRoot(ElementName = "event")]
    public class EventRedeemStatusCode
    {
        [XmlElement(ElementName = "EventCode")]
        public string? EventCode { get; set; }
    }
}
