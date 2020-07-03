//--------------------
// FILE:   LogHelper.cs
// AUTHOR: John Burns

using System;
using System.Collections.Generic;
using System.Text;

using Serilog;
using Serilog.Events;

namespace WebCrawler
{
    /// <summary>
    /// Defines static helper methods for configuration and execution
    /// of the logger.
    /// </summary>
    public static class LogHelper
    {
        /// <summary>
        /// Gets the <see cref="LogEventLevel"/> from the input level string
        /// </summary>
        /// 
        /// <param name="level">The <see cref="string"/> log level.</param>
        /// 
        /// <returns>The <see cref="LogEventLevel"/> corresponding to the input 
        /// <see cref="string"/> level.</returns>
        public static LogEventLevel GetLogLevel(string level)
        {
            if (!String.IsNullOrEmpty(level))
            {
                level = level.Trim().ToUpper();
            }

            switch (level)
            {
                case "VERBOSE":
                    return LogEventLevel.Verbose;
                case "DEBUG":
                    return LogEventLevel.Debug;
                case "INFORMATION":
                    return LogEventLevel.Information;
                case "WARNING":
                    return LogEventLevel.Warning;
                case "ERROR":
                    return LogEventLevel.Error;
                case "FATAL":
                    return LogEventLevel.Fatal;
                default:
                    return LogEventLevel.Information;
            }
        }

        /// <summary>
        /// Configures the global <see cref="Log"/>.
        /// </summary>
        /// 
        /// <param name="level">The <see cref="string"/> log level.</param>
        public static void ConfigureLogger(string level = "information")
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(GetLogLevel(level))
                .WriteTo.Console()
                .WriteTo.File("logfile.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }
    }
}
