// LogManager.cs
using Serilog;
using Serilog.Events;

namespace MuLog
{
    public interface ILogManager
    {
        bool IsInitialized { get; }

        void DisableZone(string zoneName);
        void Dispose();
        void EnableZone(string zoneName);
        ILogger GetZoneLogger(string zoneName);
        void Initialize(LoggingOptions options);
        void Write(LogEventLevel level, string message, string zone = null);
        void WriteError(Exception exception, string message, string zone = null);
    }
}