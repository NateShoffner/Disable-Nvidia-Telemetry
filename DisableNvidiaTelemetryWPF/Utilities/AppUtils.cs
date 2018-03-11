using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using DisableNvidiaTelemetry;
using DisableNvidiaTelemetryWPF.Properties;

namespace DisableNvidiaTelemetryWPF.Utilities
{
    public class AppUtils
    {
        public const string StartupParamSilent = "-silent";
        public const string StartupParamRegisterTask = "-registertask";
        public const string StartupParamUnregisterTask = "-unregistertask";


        public static ExtendedVersion.ExtendedVersion GetVersion()
        {
            var attribute =
                (AssemblyInformationalVersionAttribute) Assembly.GetExecutingAssembly()
                    .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false).FirstOrDefault();
            var version = new ExtendedVersion.ExtendedVersion(attribute.InformationalVersion);

            return version;
        }

        public static void InitializeSettings()
        {
            Settings.Default.Upgrade();
            var provider = new PortableSettingsProvider();
            Settings.Default.Providers.Add(provider);
            foreach (SettingsProperty property in Settings.Default.Properties)
            {
                property.Provider = provider;
            }
        }

        public static bool IsAdministrator()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent())
                .IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}