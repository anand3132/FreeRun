using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UnityEngine;

namespace RedGaint
{
    public enum LocalizedKey
    {
        SinglePlayer_button,
        MultiPlayer_button,
        Quit_button,
        welcome_text,
        SignIN_button,
        Profile_button
        // Add more keys here
    }
}



namespace RedGaint
{
    public static class GlobalTextBridge
    {
        private static readonly Dictionary<string, string> cachedStrings = new();
        private static readonly Dictionary<string, Action<string>> listeners = new();
        private static bool initialized = false;

        public static async void Init()
        {
            if (initialized) return;
            initialized = true;

            await LocalizationSettings.InitializationOperation.Task;
            await CacheAll();

            LocalizationSettings.SelectedLocaleChanged += async locale =>
            {
                await CacheAll();
                NotifyListeners();
            };
        }

        private static async Task CacheAll()
        {
            cachedStrings.Clear();

            foreach (var key in Enum.GetValues(typeof(LocalizedKey)))
            {
                string keyStr = key.ToString();
                var localized = new LocalizedString("UI_Text", keyStr);
                try
                {
                    var value = await localized.GetLocalizedStringAsync().Task;
                    cachedStrings[keyStr] = value;
                    Debug.Log("Localisation Enabled..."+value);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Localization load failed for key: {keyStr}, Error: {ex.Message}");
                    cachedStrings[keyStr] = $"[{keyStr}]";
                }
            }
        }

        private static void NotifyListeners()
        {
            foreach (var kvp in listeners)
            {
                if (cachedStrings.TryGetValue(kvp.Key, out string value))
                    kvp.Value?.Invoke(value);
            }
        }

        // Static string accessors for convenience
        public static string SinglePlayerButtonText => Get(LocalizedKey.SinglePlayer_button);
        public static string MultiPlayerButtonText => Get(LocalizedKey.MultiPlayer_button);
        public static string QuitButtonText => Get(LocalizedKey.Quit_button);
        public static string WelcomeText => Get(LocalizedKey.welcome_text);
        public static string SignInButtonText => Get(LocalizedKey.SignIN_button);
        public static string ProfileButtonText => Get(LocalizedKey.Profile_button);

        // Get string by enum
        public static string Get(LocalizedKey key)
        {
            Debug.Log("Get: " + key);
            string keyStr = key.ToString();
            return cachedStrings.TryGetValue(keyStr, out var value) ? value : $"[{keyStr}]";
        }

        // Register for live updates
        public static void RegisterListener(LocalizedKey key, Action<string> onValueChanged)
        {
            string keyStr = key.ToString();
            if (!listeners.ContainsKey(keyStr))
                listeners[keyStr] = _ => { };

            listeners[keyStr] += onValueChanged;

            if (cachedStrings.TryGetValue(keyStr, out string value))
                onValueChanged?.Invoke(value);
        }

        public static void UnregisterListener(LocalizedKey key, Action<string> onValueChanged)
        {
            string keyStr = key.ToString();
            if (listeners.TryGetValue(keyStr, out var existing))
            {
                listeners[keyStr] -= onValueChanged;
                if (listeners[keyStr] == null)
                    listeners.Remove(keyStr);
            }
        }
    }
}
