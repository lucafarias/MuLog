
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Concurrent;

namespace MuLog
{
    public class LogManager : IDisposable, ILogManager
    {
        private ILogger _internalLogger;
        private LoggingOptions _options;
        private static string _zonePrefix = "Zone.";
        private static readonly ConcurrentDictionary<string, bool> EnabledZones = new();
        private readonly object _lock = new object();

        public bool IsInitialized { get; private set; }

        public void Initialize(LoggingOptions options)
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException("LogManager è già stato inizializzato");
            }

            lock (_lock)
            {
                if (IsInitialized) return;

                _options = options ?? throw new ArgumentNullException(nameof(options));
                _internalLogger = CreateLogger(options);
                _zonePrefix = options.ZonePrefix ?? _zonePrefix;
                IsInitialized = true;

                _internalLogger.Information("LogManager inizializzato con livello minimo: {Level}",
                    options.DefaultMinimumLevel);
            }
        }

        private ILogger CreateLogger(LoggingOptions options)
        {
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Is(options.DefaultMinimumLevel)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Debug();

            loggerConfiguration.WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(IsZoneEnabledFilter)
                .WriteTo.Map("SourceContext", (sourceContext, wt) =>
                {
                    if (sourceContext != null && sourceContext.StartsWith(_options.ZonePrefix))
                    {
                        string zoneName = sourceContext.Substring(_options.ZonePrefix.Length);
                        wt.File($"{options.ZonedLogFileTemplatePrefix}{zoneName}-.log",
                                rollingInterval: RollingInterval.Day,
                                retainedFileCountLimit: options.ZonedLogRetentionDays);
                    }
                })
                .WriteTo.File(options.GeneralLogFileTemplate,
                              rollingInterval: RollingInterval.Day,
                              retainedFileCountLimit: options.GeneralLogRetentionDays)
            );

            return loggerConfiguration.CreateLogger();
        }

        private static bool IsZoneEnabledFilter(LogEvent logEvent)
        {
            if (logEvent.Properties.TryGetValue("SourceContext", out var sourceContextValue) &&
                sourceContextValue is ScalarValue scalar)
            {
                string context = scalar.Value?.ToString();
                if (context != null && context.StartsWith(_zonePrefix))
                {
                    string zoneName = context.Substring(_zonePrefix.Length);
                    return EnabledZones.TryGetValue(zoneName, out bool isEnabled) && isEnabled;
                }
            }
            return true;
        }

        public void EnableZone(string zoneName)
        {
            EnsureInitialized();
            if (string.IsNullOrWhiteSpace(zoneName)) return;
            EnabledZones[zoneName] = true;
            _internalLogger?.Information("Zona di logging '{ZoneName}' attivata.", zoneName);
        }

        public void DisableZone(string zoneName)
        {
            EnsureInitialized();
            if (string.IsNullOrWhiteSpace(zoneName)) return;
            EnabledZones[zoneName] = false;
            _internalLogger?.Information("Zona di logging '{ZoneName}' disattivata.", zoneName);
        }

        public ILogger GetZoneLogger(string zoneName)
        {
            EnsureInitialized();
            return _internalLogger.ForContext("SourceContext", $"{_zonePrefix}{zoneName}");
        }

        public void Write(LogEventLevel level, string message, string zone = null)
        {
            EnsureInitialized();
            if (string.IsNullOrEmpty(zone))
            {
                _internalLogger.Write(level, message);
            }
            else
            {
                GetZoneLogger(zone).Write(level, message);
            }
        }

        public void WriteError(Exception exception, string message, string zone = null)
        {
            EnsureInitialized();
            if (string.IsNullOrEmpty(zone))
            {
                _internalLogger.Error(exception, message);
            }
            else
            {
                GetZoneLogger(zone).Error(exception, message);
            }
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException(
                    "LogManager non è stato inizializzato. Chiamare Initialize() prima dell'uso.");
            }
        }

        public void Dispose()
        {
            (_internalLogger as IDisposable)?.Dispose();
        }
    }
}