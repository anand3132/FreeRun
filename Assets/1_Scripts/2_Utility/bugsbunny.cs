using UnityEngine;
using System.Diagnostics;

namespace RedGaint.Utility
{
    public interface IBugsBunny
    {
        bool LogThisClass { get; }
    }

    public static class BugsBunny
    {
        public static void Log(
            string message,
            IBugsBunny context = null,
            [System.Runtime.CompilerServices.CallerMemberName] string callerName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
        {
            if (ShouldLog(context))
            {
                DebugLog(message, callerName, callerFilePath, callerLineNumber);
            }
        }

        public static void Warning(
            string message,
            IBugsBunny context = null,
            [System.Runtime.CompilerServices.CallerMemberName] string callerName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
        {
            if (ShouldLog(context))
            {
                DebugLogWarning(message, callerName, callerFilePath, callerLineNumber);
            }
        }

        public static void LogError(
            string message,
            IBugsBunny context = null,
            [System.Runtime.CompilerServices.CallerMemberName] string callerName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
        {
            if (ShouldLog(context))
            {
                DebugLogError(message, callerName, callerFilePath, callerLineNumber);
            }
            else
            {
                LogRed($"[BugsBunny] A LogError was suppressed: {message}", context: context);
            }
        }

        public static void LogRed(string message, IBugsBunny context)
        {
            if ( ShouldLog(context))
                LogWithColor(message, "red");
        }

        public static void LogGreen(string message,IBugsBunny context = null)
        {
            if ( ShouldLog(context))
                LogWithColor(message, "green");
        }

        public static void LogBlue(string message,  IBugsBunny context = null)
        {
            if ( ShouldLog(context))
                LogWithColor(message, "blue");
        }

        public static void LogYellow(string message,  IBugsBunny context = null)
        {
            if ( ShouldLog(context))
                LogWithColor(message, "yellow");
        }

        private static void LogWithColor(string message, string color)
        {
            UnityEngine.Debug.Log($"<color={color}> {message} </color>");
        }
        // private static bool ShouldLog(IBugsBunny context)
        // {
        //     // Log only if the context is null or explicitly allows logging.
        //     return context != null && context.LogThisClass;
        // }
        private static bool ShouldLog(IBugsBunny context)
        {
            if (context == null)
                return false;
            else
            {
              return context.LogThisClass;
            }
        }
        private static void DebugLog(string message, string callerName, string callerFilePath, int callerLineNumber)
        {
            UnityEngine.Debug.Log(FormatMessage(message, callerName, callerFilePath, callerLineNumber));
        }

        private static void DebugLogWarning(string message, string callerName, string callerFilePath, int callerLineNumber)
        {
            UnityEngine.Debug.LogWarning(FormatMessage(message, callerName, callerFilePath, callerLineNumber));
        }

        private static void DebugLogError(string message, string callerName, string callerFilePath, int callerLineNumber)
        {
            UnityEngine.Debug.LogError(FormatMessage(message, callerName, callerFilePath, callerLineNumber));
        }

        private static string FormatMessage(string message, string callerName, string callerFilePath, int callerLineNumber)
        {
            string fileName = System.IO.Path.GetFileName(callerFilePath); // Extract file name from the full path
            return $"[{fileName}:{callerLineNumber} ({callerName})] {message}";
        }
    }
}
