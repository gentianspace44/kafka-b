namespace VPS.Domain.Models.Configurations
{
    public class VoucherRedeemClientNotifications
    {
        public string VoucherRedeemInProgressMessage { get; set; } = string.Empty;
        public string VoucherRedeemCriticalFailOnProducer { get; set; } = string.Empty;
        public string VoucherRedeemSuccess { get; set; } = string.Empty;
        public string VoucherRedeemCriticalFailOnConsumer { get; set; } = string.Empty;
        public string VocherRedeemFailPendingManualProcessing { get; set; } = string.Empty;
        public string VoucherAlreadyCreditedOnSyx { get; set; } = string.Empty;
    }
}
