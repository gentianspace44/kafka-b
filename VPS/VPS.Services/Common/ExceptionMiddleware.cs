using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using VPS.Domain.Models.Common;
using VPS.Helpers.Logging;

namespace VPS.Services.Common
{
    [ExcludeFromCodeCoverage]
    public class ExceptionMiddleware : IMiddleware
    {
        private readonly ILoggerAdapter<ExceptionMiddleware> _logger;
        public ExceptionMiddleware(ILoggerAdapter<ExceptionMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                await TaskHandleException(context, exception);
            }
        }

        private async Task TaskHandleException(HttpContext context, Exception exception)
        {
            var source = exception.Source;

            _logger.LogError(exception, null, "Caught by Exception Middleware - {Source}: {Exception}, Message: {Message}, StackTrace: {StackTrace}, InnerException: {InnerException}",
                MethodBase.GetCurrentMethod()?.Name ?? string.Empty,
                source?? "", exception, exception.Message, exception.StackTrace?? "", (exception.InnerException == null ? "None" : exception.InnerException));


            var response = new ServiceResponse
            {
                Amount = 0,
                CreditOutcome = -1,
                IsSuccess = false,
                Message = "An error occurred. Please try again later."
            };

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(response);

        }
    }
}
