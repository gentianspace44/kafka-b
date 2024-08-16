using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using VPS.Domain.Models.Configurations;

namespace VPS.Helpers.Logging
{
    public class LoggerAdapter<TType> : ILoggerAdapter<TType>
    {
        private readonly ILogger<TType> _logger;
        private readonly VpsLoggerConfiguration _loggerConfiguration;
        private readonly MetricsHelper _metricsHelper;

        public LoggerAdapter(ILogger<TType> logger, IOptions<VpsLoggerConfiguration> loggerConfiguration, MetricsHelper metricsHelper)
        {
            _logger = logger;
            _loggerConfiguration = loggerConfiguration.Value;
            _metricsHelper = metricsHelper;
        }

        public void LogError(string? reference, string? message, [CallerMemberName] string memberName = "",
            params object?[] args)
        {
            var fullMessage = CreateMessageStructure(reference, message, memberName);
            _metricsHelper.IncVouchersRedeemError(_logger);
            _logger.LogError(fullMessage, args);
        }

        public void LogError(Exception? exception, string? reference, string? message, [CallerMemberName] string memberName = "",
            params object?[] args)
        {
            var fullMessage = CreateMessageStructure(reference, message, memberName);
            _metricsHelper.IncVouchersRedeemError(_logger);
            _logger.LogError(exception, fullMessage, args);
        }


        public void LogWarning(string? reference, string message, [CallerMemberName] string memberName = "",
            params object?[] args)
        {

            var fullMessage = CreateMessageStructure(reference, message, memberName);

            _logger.LogWarning(fullMessage, args);

        }

        public void LogCritical(Exception? exception, string? reference, string message, [CallerMemberName] string memberName = "",
            params object?[] args)
        {

            var fullMessage = CreateMessageStructure(reference, message, memberName);
            _metricsHelper.IncVouchersRedeemCriticalError(_logger);
            _logger.LogCritical(exception, fullMessage, args);

        }

        public void LogInformation(string? reference, string message, [CallerMemberName] string memberName = "",
            params object?[] args)
        {

            var fullMessage = CreateMessageStructure(reference, message, memberName);

            _logger.LogInformation(fullMessage, args);

        }

        private string CreateMessageStructure(string? reference, string? message, string memberName)
        {

            string fullMessage;

            if (reference == null)
                fullMessage = $"{_loggerConfiguration.Provider}: {memberName} ({DateTime.Now}) - {message}";
            else
                fullMessage = $"{_loggerConfiguration.Provider}: {reference}: {memberName} ({DateTime.Now}) - {message}";

            return fullMessage;

        }
    }
}
