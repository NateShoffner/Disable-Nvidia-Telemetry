using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace DisableNvidiaTelemetry.Model
{
    internal class TelemetryRegistryKey : ITelemetry
    {
        private readonly string _subKeyPath;
        private readonly bool _useRegex;
        public Dictionary<string, RegistryExpressionModifiers> ValueExpressions;

        public Dictionary<string, RegistryValuePair> ValueStrings;

        public TelemetryRegistryKey(RegistryKey baseKey, string subKeyPath, Dictionary<string, RegistryValuePair> valueStrings)
        {
            BaseKey = baseKey;
            _subKeyPath = subKeyPath;
            ValueStrings = valueStrings;
        }

        public TelemetryRegistryKey(RegistryKey baseKey, string subKeyPath, Dictionary<string, RegistryExpressionModifiers> valueExpressions)
        {
            BaseKey = baseKey;
            _subKeyPath = subKeyPath;
            ValueExpressions = valueExpressions;
            _useRegex = true;
        }

        private RegistryKey BaseKey { get; }

        public RegistryKey SubKey => BaseKey.OpenSubKey(_subKeyPath, RegistryKeyPermissionCheck.ReadWriteSubTree);

        public string Name => $"{BaseKey.Name}\\{_subKeyPath}";

        public bool Enabled
        {
            set
            {
                var subKey = SubKey;

                if (_useRegex)
                    foreach (var vd in ValueExpressions)
                    {
                        var currentValue = subKey.GetValue(vd.Key).ToString();

                        subKey.SetValue(vd.Key, value
                            ? vd.Value.Enabled.Regex.Replace(currentValue, vd.Value.Enabled.Replacment)
                            : vd.Value.Disabled.Regex.Replace(currentValue, vd.Value.Disabled.Replacment));
                    }

                else
                    foreach (var vd in ValueStrings)
                    {
                        subKey.SetValue(vd.Key, value
                            ? vd.Value.Enabled
                            : vd.Value.Disabled);
                    }
            }
        }

        public Dictionary<string, string> GetValues()
        {
            var values = new Dictionary<string, string>();

            var subKey = SubKey;

            if (subKey == null)
                return null;

            if (_useRegex)
                foreach (var vd in ValueExpressions)
                {
                    values.Add(vd.Key, subKey.GetValue(vd.Key)?.ToString());
                }

            else
                foreach (var vd in ValueStrings)
                {
                    var value = subKey.GetValue(vd.Key);
                    values.Add(vd.Key, value?.ToString());
                }

            return values;
        }

        public class RegistryExpressionModifiers
        {
            public RegistryExpressionModifiers(Regex match, Replacement enabled, Replacement disabled)
            {
                Match = match;
                Enabled = enabled;
                Disabled = disabled;
            }

            public Regex Match { get; }

            public Replacement Enabled { get; }

            public Replacement Disabled { get; }
        }

        public class RegistryValuePair
        {
            public RegistryValuePair(string enabled, string disabled)
            {
                Enabled = enabled;
                Disabled = disabled;
            }

            public string Enabled { get; }

            public string Disabled { get; }
        }


        public class Replacement
        {
            public Replacement(Regex regex, string replacment)
            {
                Regex = regex;
                Replacment = replacment;
            }

            public Regex Regex { get; }
            public string Replacment { get; }
        }

        /// <summary>Check to see if the value exists.</summary>
        /// <returns>True if it exists, false if not.</returns>
        public bool exists()
        {
            var subKey = SubKey;

            try
            {
                if (_useRegex)
                    ValueExpressions.Select(vd => vd.Value.Match.IsMatch(subKey.GetValue(vd.Key).ToString())).FirstOrDefault();
                else
                    ValueStrings.Any(vd => subKey.GetValue(vd.Key).ToString() == vd.Value.Enabled);
            }
            catch (System.NullReferenceException)
            {
                return false;
            }

            return true;
        }

        #region Implementation of ITelemetry

        public bool IsActive()
        {
            var subKey = SubKey;

            if (subKey == null)
                return false;

            return _useRegex
                ? ValueExpressions.Select(vd => vd.Value.Match.IsMatch(subKey.GetValue(vd.Key).ToString())).FirstOrDefault()
                : ValueStrings.Any(vd => subKey.GetValue(vd.Key).ToString() == vd.Value.Enabled);
        }

        public bool RestartRequired { get; set; }

        #endregion
    }
}