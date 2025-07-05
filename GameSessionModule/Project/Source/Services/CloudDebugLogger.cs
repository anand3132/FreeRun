using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace RedGaint.Network.GameSessionModule
{
    public static class CloudDebugLogger
    {
        private static readonly string LogFilePath = "/tmp/cloudcode_debug_log.txt";
        private const int MaxReturnLength = 4000;

        // Backing ILogger instance
        private static ILogger? _logger;

        /// <summary>
        /// Call this once during startup to enable ILogger logging
        /// </summary>
        public static void Initialize(ILogger logger)
        {
            _logger = logger;
        }

        public static void LogInfo(string message)
        {
            _logger?.LogInformation(message);
            LogInternal("INFO", message);
        }

        public static void LogWarning(string message)
        {
            _logger?.LogWarning(message);
            LogInternal("WARN", message);
        }

        public static void LogError(string message)
        {
            _logger?.LogError(message);
            LogInternal("ERROR", message);
        }

        public static void LogError(Exception ex, string message = "")
        {
            var errorMessage = $"{message}\nException: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
            _logger?.LogError(ex, message);
            LogInternal("ERROR", errorMessage);
        }

        private static void LogInternal(string level, string message)
        {
            try
            {
                string line = $"[{DateTime.UtcNow:HH:mm:ss}] [{level}] {message}";
                File.AppendAllText(LogFilePath, line + Environment.NewLine);
            }
            catch
            {
                // Silent fail
            }
        }

        public static string GetRecentLogs()
        {
            if (!File.Exists(LogFilePath))
                return "No log file found.";

            try
            {
                var content = File.ReadAllText(LogFilePath);
                return content.Length <= MaxReturnLength
                    ? content
                    : content.Substring(content.Length - MaxReturnLength);
            }
            catch
            {
                return "Error reading log file.";
            }
        }

        public static void Clear()
        {
            try
            {
                if (File.Exists(LogFilePath))
                    File.Delete(LogFilePath);
            }
            catch { }
        }
    }
}
