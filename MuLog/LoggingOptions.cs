using Serilog.Events;

namespace MuLog
{
    public class LoggingOptions
    {
        // Configurazioni per i File Generici
        public string GeneralLogFileTemplate { get; set; } = "logs/general-.log";
        public int GeneralLogRetentionDays { get; set; } = 30;
        public string ZonePrefix = "Zones.";
        // Configurazioni per i File delle Zone Dinamiche
        public string ZonedLogFileTemplatePrefix { get; set; } = "logs/";
        public int ZonedLogRetentionDays { get; set; } = 7;

        // Livello minimo globale di logging (es. Information, Debug, Warning)
        // Si applica a tutti i log che passano i filtri di zona.
        public LogEventLevel DefaultMinimumLevel { get; set; } = LogEventLevel.Information;
    }
}
