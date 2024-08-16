using System.Xml.Serialization;

namespace VPS.Domain.Models.BluVoucher.Responses
{
    [XmlRoot(ElementName = "Status")]
    public class RedeemEventDetails
    {
        [XmlElement(ElementName = "Code")]
        public string? Code { get; set; }
        [XmlElement(ElementName = "Description")]
        public string? Description { get; set; }
        [XmlElement(ElementName = "Amount")]
        public decimal Amount { get; set; }
        [XmlElement(ElementName = "RedemtionTransRef")]
        public string? RedemtionTransRef { get; set; }
        [XmlElement(ElementName = "RedemtionDate")]
        public string? RedemtionDate { get; set; }
    }
}
