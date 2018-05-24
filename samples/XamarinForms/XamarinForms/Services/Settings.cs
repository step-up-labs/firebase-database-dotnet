using System;
using System.Collections.Generic;
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace XamarinForms.Services
{
    /// <summary>
    /// This is the Settings static class that can be used in your Core solution or in any
    /// of your client applications. All settings are laid out the same exact way with getters
    /// and setters. 
    /// </summary>
    public static class Settings
    {
        private static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }

        public static string FirebaseAuthJson
        {
            get => AppSettings.GetValueOrDefault(nameof(FirebaseAuthJson), string.Empty);
            set => AppSettings.AddOrUpdateValue(nameof(FirebaseAuthJson), value);
        }
    }
}
