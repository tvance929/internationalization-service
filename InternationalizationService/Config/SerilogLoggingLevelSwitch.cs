using Serilog.Core;
using Serilog.Events;

namespace InternationalizationService.Config
{
    /// <summary>
    /// Serilog Logging Level Switch to allow for dynamic changes to logging level at runtime
    /// </summary>
    public static class SerilogLoggingLevelSwitch
    {
        /// <summary>
        /// The instance of <see cref="LoggingLevelSwitch"/> type.
        /// </summary>
        public static LoggingLevelSwitch LevelSwitch = new LoggingLevelSwitch();

        /// <summary>
        /// Set minimum logging level
        /// </summary>
        /// <param name="eventLevel">log event level</param>
        public static void SetLoggingLevel(int eventLevel)
        {
            switch (eventLevel)
            {
                case 0:
                    LevelSwitch.MinimumLevel = LogEventLevel.Verbose;
                    break;
                case 1:
                    LevelSwitch.MinimumLevel = LogEventLevel.Debug;
                    break;
                case 2:
                    LevelSwitch.MinimumLevel = LogEventLevel.Information;
                    break;
                case 3:
                    LevelSwitch.MinimumLevel = LogEventLevel.Warning;
                    break;
                case 4:
                    LevelSwitch.MinimumLevel = LogEventLevel.Error;
                    break;
                case 5:
                    LevelSwitch.MinimumLevel = LogEventLevel.Fatal;
                    break;
            }
        }
    }
}
