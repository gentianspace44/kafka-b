using System.ComponentModel.DataAnnotations;

namespace VPS.Domain.Models.RACellularVoucher.Requests;
public class RaCellularProviderVoucherRequest
{
    [MaxLength(20)]
    public string pinNumber { get; set; } = string.Empty;
    [MaxLength(45)]
    public string msgID { get; set; } = string.Empty;
    [StringLength(45)]
    public string terminalID { get; set; } = string.Empty;
    [StringLength(255)]
    public string reason { get; set; } = string.Empty;
    [StringLength(45)]
    public string terminalOperator { get; set; } = string.Empty;
    [StringLength(45)]
    public string adviceMsgID { get; set; } = string.Empty;
}
