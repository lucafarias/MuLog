var lm = new MuLog.LogManager();

lm.Initialize(new MuLog.LoggingOptions
{
    DefaultMinimumLevel = Serilog.Events.LogEventLevel.Information,
    GeneralLogFileTemplate = "logs/general-.log",
    ZonedLogFileTemplatePrefix = "logs/zones/",
    ZonedLogRetentionDays = 7
});

lm.Write(Serilog.Events.LogEventLevel.Information, "This is a test log message.");
lm.EnableZone("MAZZA");
lm.Write(Serilog.Events.LogEventLevel.Information, "This is a test log message.", "MAZZA");

