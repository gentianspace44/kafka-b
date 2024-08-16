using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VPS.Domain.Models.Flash.Responses
{
    [XmlRoot(ElementName = "response")]
    public class FlashRedeemResponse
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string StoreId { get; set; } = string.Empty;
        public string TerminalId { get; set; } = string.Empty;
        public Voucher Voucher { get; set; } = new Voucher();
        public int ResponseCode { get; set; }
        public string ResponseMessage { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public int Amount { get; set; }
        public string TransactionDate { get; set; } = string.Empty;
    }
 
    public class Voucher
    {
        public int Amount { get; set; }
        public string ExpiryDate { get; set; } = string.Empty;
        public string Pin { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public Content Content { get; set; } = new Content();
    }
    public class Content
    {
        public string RedemptionInstructions { get; set; } = string.Empty;
        public string TermsAndConditions { get; set; } = string.Empty;
    }
}
