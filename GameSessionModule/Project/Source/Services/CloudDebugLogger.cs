using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RedGaint.Network.GameSessionModule
{
    public static class CloudDebugLogger
    {
        private static readonly string LogFilePath = "/tmp/cloudcode_debug_log.txt";
        private const int MaxReturnLength = 4000; // Max characters to return when queried

        /// <summary>
        /// Appends a line to the debug log file.
        /// </summary>
        public static void Log(string message)
        {
            try
            {
                string line = $"[{DateTime.UtcNow:HH:mm:ss}] {message}";
                File.AppendAllText(LogFilePath, line + Environment.NewLine);
            }
            catch
            {
                // Intentionally suppress file errors
            }
        }

        /// <summary>
        /// Reads the last part of the log file, up to MaxReturnLength.
        /// </summary>
        public static string GetRecentLogs()
        {
            if (!File.Exists(LogFilePath))
                return "No log file found.";

            try
            {
                var content = File.ReadAllText(LogFilePath);
                if (content.Length <= MaxReturnLength)
                    return content;

                return content.Substring(content.Length - MaxReturnLength);
            }
            catch
            {
                return "Error reading log file.";
            }
        }

        /// <summary>
        /// Clears the log file (optional).
        /// </summary>
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
