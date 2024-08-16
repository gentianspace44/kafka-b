using System.Xml.Serialization;

namespace VPS.Domain.Models.BluVoucher.Requests
{
    [XmlRoot(ElementName = "request")]
    public class AuthenticationRequest
    {

        [XmlElement(ElementName = "EventType")]
        public string? EventType { get; set; }

        [XmlElement(ElementName = "event")]
        public RequestEvent? RequestEvent { get; set; }

    }

    [XmlRoot(ElementName = "event")]
    public class RequestEvent
    {

        [XmlElement(ElementName = "UserPin")]
        public string? UserPin { get; set; }
        [XmlElement(ElementName = "DeviceId")]
        public string? DeviceId { get; set; }
        [XmlElement(ElementName = "DeviceSer")]
        public string? DeviceSer { get; set; }
        [XmlElement(ElementName = "TransType")]
        public string? TransType { get; set; }
        [XmlElement(ElementName = "Reference")]
        public string? Reference { get; set; }

    }

    [XmlRoot(ElementName = "request")]
    public class ElectricityAuthenticationRequest
    {

        [XmlElement(ElementName = "EventType")]
        public string? EventType { get; set; }

        [XmlElement(ElementName = "event")]
        public ElectricityRequestEvent? ElectricityRequestEvent { get; set; }

    }

    [XmlRoot(ElementName = "event")]
    public class ElectricityRequestEvent : RequestEvent
    {

        [XmlElement(ElementName = "MeterNum")]
        public string? MeterNum { get; set; }

        [XmlElement(ElementName = "Amount")]
        public string? Amount { get; set; }

    }
}
