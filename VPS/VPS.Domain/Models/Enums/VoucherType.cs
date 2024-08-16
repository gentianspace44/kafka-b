using System.ComponentModel;

namespace VPS.Domain.Models.Enums;

public enum VoucherType
{

    //match the db
    [Description("HollyTopUp")]
    HollyTopUp = 1,

    [Description("OTT")]
    OTT = 2,

    [Description("Flash_OneVoucher")]
    Flash = 3,

    [Description("BluVoucher")]
    BluVoucher = 4,

    [Description("EasyLoad")]
    EasyLoad = 5,

    [Description("RACellular")]
    RACellular = 6,

    [Description("HollaMobile")]
    HollaMobile = 7,

}
