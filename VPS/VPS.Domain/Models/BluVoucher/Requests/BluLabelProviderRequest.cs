using System.Xml.Serialization;

namespace VPS.Domain.Models.BluVoucher.Requests
{
    [XmlRoot(ElementName = "request")]
    public class BluLabelProviderRequest
    {
        [XmlElement(ElementName = "SessionId")]
        public string? SessionId { get; set; }
        [XmlElement(ElementName = "EventType")]
        public string? EventType { get; set; }
        [XmlElement(ElementName = "event")]
        public AirtimeRequestEvent? Event { get; set; }
    }
}
