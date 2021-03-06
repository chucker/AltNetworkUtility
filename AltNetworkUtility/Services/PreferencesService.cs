using System;

using Xamarin.Essentials;

namespace AltNetworkUtility.Services
{
    /// <summary>
    /// This is just a wrapper around Xamarin.Essentials so that we can prefix
    /// our preferences with type names.
    /// </summary>
    public class PreferencesService
    {
        public string Prefix { get; private set; }

        public string MakeKey(string key) => $"{Prefix}.{key}";

        /// <param name="key">Preference key.</param>
        /// <param name="defaultValue">Default value to return if the key does not exist.</param>
        /// <summary>Gets the value for a given key, or the default specified if the key does not exist.</summary>
        /// <returns>Value for the given key, or the default if it does not exist.</returns>
        /// <remarks />
        public bool Get(string key, bool defaultValue)
            => Preferences.Get(MakeKey(key), defaultValue);

        /// <param name="key">Preference key.</param>
        /// <param name="defaultValue">Default value to return if the key does not exist.</param>
        /// <summary>Gets the value for a given key, or the default specified if the key does not exist.</summary>
        /// <returns>Value for the given key, or the default if it does not exist.</returns>
        /// <remarks />
        public int Get(string key, int defaultValue)
            => Preferences.Get(MakeKey(key), defaultValue);

        /// <param name="key">Preference key.</param>
        /// <param name="defaultValue">Default value to return if the key does not exist.</param>
        /// <summary>Gets the value for a given key, or the default specified if the key does not exist.</summary>
        /// <returns>Value for the given key, or the default if it does not exist.</returns>
        /// <remarks />
        public string Get(string key, string defaultValue)
            => Preferences.Get(MakeKey(key), defaultValue);

        /// <param name="key">Preference key.</param>
        /// <param name="defaultValue">Default value to return if the key does not exist.</param>
        /// <summary>Gets the value for a given key, or the default specified if the key does not exist.</summary>
        /// <returns>Value for the given key, or the default if it does not exist.</returns>
        /// <remarks />
        public TEnum GetEnum<TEnum>(string key, TEnum defaultValue) where TEnum : struct
        {
            var stringVal = Preferences.Get(MakeKey(key), Enum.GetName(typeof(TEnum), defaultValue));

            if (!Enum.TryParse<TEnum>(stringVal, out var result))
                return defaultValue;

            return result;
        }

        /// <param name="key">Preference key.</param>
        /// <summary>Removes a key and its associated value if it exists.</summary>
        /// <remarks />
        public void Remove(string key) => Preferences.Remove(key);

        /// <param name="key">Preference key.</param>
        /// <param name="value">Preference value.</param>
        /// <summary>Sets a value for a given key.</summary>
        /// <remarks />
        public void Set(string key, bool value)
            => Preferences.Set(MakeKey(key), value);

        /// <param name="key">Preference key.</param>
        /// <param name="value">Preference value.</param>
        /// <summary>Sets a value for a given key.</summary>
        /// <remarks />
        public void Set(string key, int value)
            => Preferences.Set(MakeKey(key), value);

        /// <param name="key">Preference key.</param>
        /// <param name="value">Preference value.</param>
        /// <summary>Sets a value for a given key.</summary>
        /// <remarks />
        public void Set(string key, string value)
            => Preferences.Set(MakeKey(key), value);

        private PreferencesService(string prefix) => Prefix = prefix;

        public static PreferencesService GetInstance<T>()
            => new(prefix: typeof(T).Name);
    }
}
