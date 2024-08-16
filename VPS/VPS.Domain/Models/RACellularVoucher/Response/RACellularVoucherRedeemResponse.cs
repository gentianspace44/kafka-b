namespace VPS.Domain.Models.RACellularVoucher.Response;

public class RaCellularVoucherRedeemResponse
{
    public Type? Name { get; set; }
    public string MsgID { get; set; } = string.Empty;
    public bool HasFault { get; set; }
    public string FaultNumber { get; set; } = string.Empty;
    public string FaultDesc { get; set; } = string.Empty;
    public string FaultMsg { get; set; } = string.Empty;
    public string FaultALR { get; set; } = string.Empty;
    public string PinNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TID { get; set; } = string.Empty;
    public string Serial { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public decimal PaymentFee { get; set; }
}
