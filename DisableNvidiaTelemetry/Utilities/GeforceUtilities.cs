using System;
using System.Linq;
using Microsoft.Win32;

namespace DisableNvidiaTelemetry.Utilities
{
    internal class GeforceUtilities
    {
        public static string GetGeforceExperiencePath()
        {
            using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))

            using (var key = hklm.OpenSubKey(@"SOFTWARE\NVIDIA Corporation\Global\GFExperience"))
            {
                if (key != null)
                {
                    var path = key.GetValue("FullPath");

                    if (path != null)
                        return path.ToString();
                }
            }

            return null;
        }

        public static ExtendedVersion.ExtendedVersion GetGeForceExperienceVersion()
        {
            return GetApplicationVersion("NVIDIA GeForce Experience");
        }

        public static ExtendedVersion.ExtendedVersion GetDriverVersion()
        {
            return GetApplicationVersion("NVIDIA Graphics Driver");
        }

        private static ExtendedVersion.ExtendedVersion GetApplicationVersion(string displayNamePrefix)
        {
            using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))

            using (var key = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"))
            {
                foreach (var subkey_name in key.GetSubKeyNames())
                {
                    using (var subkey = key.OpenSubKey(subkey_name))
                    {
                        var name = subkey.GetValue("DisplayName");

                        if (name != null)
                        {
                            var nameStr = name.ToString();

                            if (!string.IsNullOrEmpty(nameStr))
                                if (nameStr.IndexOf(displayNamePrefix, StringComparison.InvariantCultureIgnoreCase) >= 0)
                                {
                                    var split = nameStr.Split(' ');
                                    var versionStr = split.Last();

                                    return new ExtendedVersion.ExtendedVersion(versionStr);
                                }
                        }
                    }
                }
            }

            return null;
        }
    }
}