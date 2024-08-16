using System.Xml.Serialization;

namespace VPS.Domain.Models.BluVoucher.Responses
{
    [XmlRoot(ElementName = "response")]
    public class AirtimeAuthenticationResponse
    {
        [XmlElement(ElementName = "SessionId")]
        public string? SessionId { get; set; }
        [XmlElement(ElementName = "EventType")]
        public string? EventType { get; set; }
        [XmlElement(ElementName = "event")]
        public AuthenticationEvent? Event { get; set; }
        [XmlElement(ElementName = "data")]
        public AuthenticationData? Data { get; set; }
    }

    [XmlRoot(ElementName = "event")]
    public class AuthenticationEvent
    {
        [XmlElement(ElementName = "EventCode")]
        public string? EventCode { get; set; }
    }

    [XmlRoot(ElementName = "data")]
    public class AuthenticationData
    {
        [XmlElement(ElementName = "TransTypes")]
        public AuthenticationTransTypes? TransTypes { get; set; }
        [XmlElement(ElementName = "Reference")]
        public string? Reference { get; set; }
    }

    [XmlRoot(ElementName = "TransTypes")]
    public class AuthenticationTransTypes
    {
        [XmlElement(ElementName = "TransType")]
        public List<string>? TransType { get; set; } 
    }
}
