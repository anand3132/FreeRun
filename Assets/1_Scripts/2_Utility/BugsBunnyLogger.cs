using UnityEngine;
using System.Diagnostics;
using System;
using System.Linq;

namespace RedGaint.Utility
{
    public interface IBugsBunny
    {
        // Usage:
        // Implement this property in your class like this:
        // public bool LogThisClass => true;
        //
        // This enables conditional logging using BugsBunny.Log(...)
        // Example:
        //     if (LogThisClass)
        //         BugsBunny.Log("Something happened", this);
        //
        // Note: Logging will only occur if this returns true.

        bool LogThisClass { get; }
    }

    public static class BugsBunnyLogger
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
                DebugLog(message, callerName, callerFilePath, callerLineNumber, context);
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
                DebugLogWarning(message, callerName, callerFilePath, callerLineNumber, context);
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
                DebugLogError(message, callerName, callerFilePath, callerLineNumber, context);
            }
            else
            {
                LogRed($"[BugsBunny] A LogError was suppressed: {message}", context);
            }
        }

        public static void LogRed(string message, IBugsBunny context)
        {
            if (ShouldLog(context))
                LogWithColor(message, "red", context);
        }

        public static void LogGreen(string message, IBugsBunny context = null)
        {
            if (ShouldLog(context))
                LogWithColor(message, "green", context);
        }

        public static void LogBlue(string message, IBugsBunny context = null)
        {
            if (ShouldLog(context))
                LogWithColor(message, "blue", context);
        }

        public static void LogYellow(string message, IBugsBunny context = null)
        {
            if (ShouldLog(context))
                LogWithColor(message, "yellow", context);
        }

        private static void LogWithColor(string message, string color, IBugsBunny context)
        {
            UnityEngine.Debug.Log($"<color={color}>{message}</color>", GetUnityContextObject(context));
        }

        private static bool ShouldLog(IBugsBunny context)
        {
            return context != null && context.LogThisClass;
        }
        
        [System.Diagnostics.DebuggerHidden]
        private static void DebugLog(string message, string callerName, string callerFilePath, int callerLineNumber, IBugsBunny context)
        {
            UnityEngine.Debug.Log(
                FormatMessage(message, callerName, callerFilePath, callerLineNumber),
                GetUnityContextObject(context));
        }

        private static void DebugLogWarning(string message, string callerName, string callerFilePath, int callerLineNumber, IBugsBunny context)
        {
            UnityEngine.Debug.LogWarning(
                FormatMessage(message, callerName, callerFilePath, callerLineNumber),
                GetUnityContextObject(context));
        }

        private static void DebugLogError(string message, string callerName, string callerFilePath, int callerLineNumber, IBugsBunny context)
        {
            UnityEngine.Debug.LogError(
                FormatMessage(message, callerName, callerFilePath, callerLineNumber),
                GetUnityContextObject(context));
        }

        private static string FormatMessage(string message, string callerName, string callerFilePath, int callerLineNumber)
        {
            string fileName = System.IO.Path.GetFileName(callerFilePath);
            return $"[{fileName}:{callerLineNumber} ({callerName})] {message}";
        }

        private static UnityEngine.Object GetUnityContextObject(IBugsBunny context)
        {
            // Attempt to cast to MonoBehaviour for stack trace linking
            return context as UnityEngine.Object;
        }
    }
}
