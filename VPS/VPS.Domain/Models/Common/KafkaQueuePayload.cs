using VPS.Domain.Models.Enums;

namespace VPS.Domain.Models.Common
{
    public class KafkaQueuePayload<T> where T : class
    {
        public T? VoucherRedeemRequest { get; set; }
        public VoucherType VoucherType { get; set; }
        public RedeemOutcome? RedeemOutcome { get; set; }
    }
}
