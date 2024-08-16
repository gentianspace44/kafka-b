using System.Xml.Serialization;

namespace VPS.Domain.Models.BluVoucher.Responses
{
    [XmlRoot(ElementName = "response")]
    public class BluVoucherProviderXmlResponse
    {
        [XmlElement(ElementName = "SessionId")]
        public string? SessionId { get; set; }
        [XmlElement(ElementName = "EventType")]
        public string? EventType { get; set; }
        [XmlElement(ElementName = "event")]
        public EventRedeemStatusCode? Event { get; set; }
        [XmlElement(ElementName = "data")]
        public RedeemData? Data { get; set; }
    }
}
