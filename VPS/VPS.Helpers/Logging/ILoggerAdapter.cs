using System.Runtime.CompilerServices;

namespace VPS.Helpers.Logging
{
    public interface ILoggerAdapter<TType>
    {
        void LogError(string? reference, string? message, [CallerMemberName] string memberName = "", params object?[] args);
        void LogError(Exception? exception, string? reference, string? message, [CallerMemberName] string memberName = "",
            params object?[] args);

        void LogWarning(string? reference, string message, [CallerMemberName] string memberName = "",
            params object?[] args);

        void LogCritical(Exception? exception, string? reference, string message, [CallerMemberName] string memberName = "",
            params object?[] args);

        void LogInformation(string? reference, string message, [CallerMemberName] string memberName = "", params object?[] args);

    }
}
