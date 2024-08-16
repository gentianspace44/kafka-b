using Microsoft.Extensions.Logging;
using Prometheus;
using VPS.Helpers.Logging;

namespace VPS.Helpers
{
    public class MetricsHelper
    {
        public readonly Counter VouchersRedeemInitiated = Metrics.CreateCounter("vps_voucher_redemption_initiated_total", "Total Voucher Redemption Initiated");
        public readonly Counter VouchersRedeemProducedOnKafka = Metrics.CreateCounter("vps_voucher_redemption_produced_on_kafka_total", "Total Voucher Redemption Produced On Kafka");
        public readonly Counter VouchersRedeemConsumedOnKafka = Metrics.CreateCounter("vps_voucher_redemption_consumed_on_kafka_total", "Total Voucher Redemption Consumed On Kafka");        
        private readonly Counter VouchersRedeemError = Metrics.CreateCounter("vps_voucher_redemption_error_total", "Total Voucher Redemption Error");
        private readonly Counter VouchersRedeemCriticalError = Metrics.CreateCounter("vps_voucher_redemption_critical_error_total", "Total Voucher Redemption Critical Error");
        private readonly Counter VouchersRedeemFailed = Metrics.CreateCounter("vps_voucher_redemption_failure_total", "Total Voucher Redemption Failed");
        private readonly Counter InvalidVouchersFormat = Metrics.CreateCounter("vps_invalid_voucher_format_total", "Total Invalid Voucher Format");  
        private readonly Counter VouchersScheduledForManualProcessing = Metrics.CreateCounter("vps_scheduled_for_manual_Processing_total", "Total Vouchers Scheduled For Manual Processing");
        private readonly Counter VouchersConcurrentRequest = Metrics.CreateCounter("vps_concurrent_request_total", "Total Concurrent Request");
        private readonly Counter VouchersAbortedByDelayPolicy = Metrics.CreateCounter("vps_vouchers_aborted_by_delay_policy_total", "Total Vouchers Aborted By Delay Policy");


        private readonly Counter BluVoucherRequestCounter = Metrics.CreateCounter("vps_bluevoucher_request_total", "Total Vouchers Redeem Request For BluVoucher");
        private readonly Counter EasyLoadRequestCounter = Metrics.CreateCounter("vps_easyload_request_total", "Total Vouchers Redeem Request For EasyLoad");
        private readonly Counter OTTRequestCounter = Metrics.CreateCounter("vps_ott_request_total", "Total Vouchers Redeem Request For OTT");
        private readonly Counter HollyTopUpRequestCounter = Metrics.CreateCounter("vps_hollytopup_request_total", "Total Vouchers Redeem Request For HollyTopUp");
        private readonly Counter HollaMobileUpRequestCounter = Metrics.CreateCounter("vps_hollaMobile_request_total", "Total Vouchers Redeem Request For HollaMobile");
        private readonly Counter FlashRequestCounter = Metrics.CreateCounter("vps_flash_request_total", "Total Vouchers Redeem Request For Flash");
        private readonly Counter RAVoucherRequestCounter = Metrics.CreateCounter("vps_ravoucher_request_total", "Total Vouchers Redeem Request For R&AVoucher");

        private readonly Counter BluVoucherSuccessCounter = Metrics.CreateCounter("vps_bluevoucher_success_total", "Total Successful Vouchers Redeem For BluVoucher");
        private readonly Counter EasyLoadSuccessCounter = Metrics.CreateCounter("vps_easyload_success_total", "Total Successful Vouchers Redeem For EasyLoad");
        private readonly Counter OTTSuccessCounter = Metrics.CreateCounter("vps_ott_success_total", "Total Successful Vouchers Redeem For OTT");
        private readonly Counter HollyTopUpSuccessCounter = Metrics.CreateCounter("vps_hollytopup_success_total", "Total Successful Vouchers Redeem For HollyTopUp");
        private readonly Counter HollaMobileUpSuccessCounter = Metrics.CreateCounter("vps_hollaMobile_success_total", "Total Successful Vouchers Redeem For HollaMobile");
        private readonly Counter FlashSuccessCounter = Metrics.CreateCounter("vps_flash_success_total", "Total Successful Vouchers Redeem For Flash");
        private readonly Counter RAVoucherSuccessCounter = Metrics.CreateCounter("vps_ravoucher_success_total", "Total Successful Vouchers Redeem For R&AVoucher");



        // Histograms
        public readonly Histogram providerBlueVoucherApiResponse = Metrics.CreateHistogram("vps_bluvoucher_api_call_duration_seconds", "Histogram of bluvoucher provider api response durations.",
            new HistogramConfiguration
            {
                Buckets = new double[] { 0.001, 0.01, 0.1, 0.3, 0.5, 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 6, 7, 8, 9 }
            });

        public readonly Histogram providerEasyLoadApiResponse = Metrics.CreateHistogram("vps_easyload_api_call_duration_seconds", "Histogram of easyload provider api response durations.",
          new HistogramConfiguration
          {
              Buckets = new double[] { 0.001, 0.01, 0.1, 0.3, 0.5, 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 6, 7, 8, 9 }
          });

        public readonly Histogram providerFlashApiResponse = Metrics.CreateHistogram("vps_flash_api_call_duration_seconds", "Histogram of flash provider api response durations.",
        new HistogramConfiguration
        {
            Buckets = new double[] { 0.001, 0.01, 0.1, 0.3, 0.5, 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 6, 7, 8, 9 }
        });

        public readonly Histogram providerOTTApiResponse = Metrics.CreateHistogram("vps_ott_api_call_duration_seconds", "Histogram of ott provider api response durations.",
           new HistogramConfiguration
           {
               Buckets = new double[] { 0.001, 0.01, 0.1, 0.3, 0.5, 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 6, 7, 8, 9 }
           });

        public readonly Histogram providerRAVoucherApiResponse = Metrics.CreateHistogram("vps_ravoucher_api_call_duration_seconds", "Histogram of ravoucher provider api response durations.",
        new HistogramConfiguration
        {
            Buckets = new double[] { 0.001, 0.01, 0.1, 0.3, 0.5, 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 6, 7, 8, 9 }
        });

        public readonly Histogram databaseResponse = Metrics.CreateHistogram("vps_database_call_duration_seconds", "Histogram of database response durations.",
           new HistogramConfiguration
           {
               Buckets = new double[] { 0.001, 0.01, 0.1, 0.3, 0.5, 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 6, 7, 8, 9 }
           });

        public readonly Histogram kafkaProducerResponse = Metrics.CreateHistogram("vps_kafka_producer_duration_seconds", "Histogram of kafka producer response durations.",
        new HistogramConfiguration
        {
            Buckets = new double[] { 0.001, 0.01, 0.1, 0.3, 0.5, 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 6, 7, 8, 9 }
        });

        public readonly Histogram syxAPIResponse = Metrics.CreateHistogram("vps_syx_api_update_client_balance_response_seconds", "Histogram of Syx update client balance response durations.",
          new HistogramConfiguration
          {
              Buckets = new double[] { 0.001, 0.01, 0.1, 0.3, 0.5, 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 6, 7, 8, 9 }
          });

        public readonly Histogram redisResponse = Metrics.CreateHistogram("vps_redis_response_seconds", "Histogram of redis response durations.",
         new HistogramConfiguration
         {
             Buckets = new double[] { 0.001, 0.01, 0.1, 0.3, 0.5, 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 6, 7, 8, 9 }
         });

        public void IncVouchersRedeemInitiated<T>(ILoggerAdapter<T> _logger)
        {
            VouchersRedeemInitiated.Inc();
            _logger.LogInformation(null, "VouchersRedeemInitiated_VPS");
        }

        public void IncVouchersRedeemProducedOnKafka<T>(ILoggerAdapter<T> _logger)
        {
            VouchersRedeemProducedOnKafka.Inc();
            _logger.LogInformation(null, "VouchersRedeemProducedOnKafka_VPS");
        }

        public void IncVouchersRedeemError<T>(Microsoft.Extensions.Logging.ILogger<T> _logger)
        {
            VouchersRedeemError.Inc();
            _logger.LogInformation(null, "VouchersRedeemError_VPS");
        }

        public void IncVouchersRedeemCriticalError<T>(Microsoft.Extensions.Logging.ILogger<T> _logger)
        {
            VouchersRedeemCriticalError.Inc();
            _logger.LogInformation(null, "VouchersRedeemCriticalError_VPS");
        }

        public void IncVouchersRedeemFailed<T>(ILoggerAdapter<T> _logger)
        {
            VouchersRedeemFailed.Inc();
            _logger.LogInformation(null, "VouchersRedeemFailed_VPS");
        }

        public void IncInvalidVouchersFormat<T>(ILoggerAdapter<T> _logger)
        {
            InvalidVouchersFormat.Inc();
            _logger.LogInformation(null, "InvalidVouchersFormat_VPS");
        }

        public void IncVouchersRedeemConsumedOnKafka<T>(ILoggerAdapter<T> _logger)
        {
            VouchersRedeemConsumedOnKafka.Inc();
            _logger.LogInformation(null, "VouchersRedeemConsumedOnKafka_VPS");
        }

        public void IncVouchersScheduledForManualProcessing<T>(ILoggerAdapter<T> _logger)
        {
            VouchersScheduledForManualProcessing.Inc();
            _logger.LogInformation(null, "VouchersScheduledForManualProcessing_VPS");
        }

        public void IncBluVoucherRequestCounter<T>(ILoggerAdapter<T> _logger)
        {
            BluVoucherRequestCounter.Inc();
            _logger.LogInformation(null, "BluVoucherRequestCounter_VPS");
        }

        public void IncEasyLoadRequestCounter<T>(ILoggerAdapter<T> _logger) {
            EasyLoadRequestCounter.Inc();
            _logger.LogInformation(null, "EasyLoadRequestCounter_VPS");
        }

        public void IncOTTRequestCounter<T>(ILoggerAdapter<T> _logger) {
            OTTRequestCounter.Inc();
            _logger.LogInformation(null, "OTTRequestCounter_VPS");
        }

        public void IncHollyTopUpRequestCounter<T>(ILoggerAdapter<T> _logger) {
            HollyTopUpRequestCounter.Inc();
            _logger.LogInformation(null, "HollyTopUpRequestCounter_VPS");
        }
        
        public void IncHollaMobileRequestCounter<T>(ILoggerAdapter<T> _logger) {
            HollaMobileUpRequestCounter.Inc();
            _logger.LogInformation(null, "HollaMobileUpRequestCounter_VPS");
        }

        public void IncFlashRequestCounter<T>(ILoggerAdapter<T> _logger) {
            FlashRequestCounter.Inc();
            _logger.LogInformation(null, "FlashRequestCounter_VPS");
        }

        public void IncRAVoucherRequestCounter<T>(ILoggerAdapter<T> _logger) {
            RAVoucherRequestCounter.Inc();
            _logger.LogInformation(null, "RAVoucherRequestCounter_VPS");
        }

        public void IncVouchersConcurrentRequest<T>(ILoggerAdapter<T> _logger)
        {
            VouchersConcurrentRequest.Inc();
            _logger.LogInformation(null, "VouchersConcurrentRequest_VPS");
        }

        public void IncVouchersAbortedByDelayPolicy<T>(ILoggerAdapter<T> _logger)
        {
            VouchersAbortedByDelayPolicy.Inc();
            _logger.LogInformation(null, "VouchersAbortedByDelayPolicy_VPS");
        }

        public void IncBluVoucherSuccessCounter<T>(ILoggerAdapter<T> _logger)
        {
            BluVoucherSuccessCounter.Inc();
            _logger.LogInformation(null, "BluVoucherSuccessCounter_VPS");
        }

        public void IncEasyLoadSuccessCounter<T>(ILoggerAdapter<T> _logger)
        {
            EasyLoadSuccessCounter.Inc();
            _logger.LogInformation(null, "EasyLoadSuccessCounter_VPS");
        }

        public void IncOTTSuccessCounter<T>(ILoggerAdapter<T> _logger)
        {
            OTTSuccessCounter.Inc();
            _logger.LogInformation(null, "OTTSuccessCounter_VPS");
        }


        public void IncHollyTopUpSuccessCounter<T>(ILoggerAdapter<T> _logger)
        {
            HollyTopUpSuccessCounter.Inc();
            _logger.LogInformation(null, "HollyTopUpSuccessCounter_VPS");
        }

        public void IncHollaMobileUpSuccessCounter<T>(ILoggerAdapter<T> _logger)
        {
            HollaMobileUpSuccessCounter.Inc();
            _logger.LogInformation(null, "HollaMobileUpSuccessCounter_VPS");
        }

        public void IncFlashSuccessCounter<T>(ILoggerAdapter<T> _logger)
        {
            FlashSuccessCounter.Inc();
            _logger.LogInformation(null, "FlashSuccessCounter_VPS");
        }

        public void IncRAVoucherSuccessCounter<T>(ILoggerAdapter<T> _logger)
        {
            RAVoucherSuccessCounter.Inc();
            _logger.LogInformation(null, "RAVoucherSuccessCounter_VPS");
        }
    }
}
