using System;

namespace DisableNvidiaTelemetry.Model
{
    /// <summary> Represents an exception where a registry key could not be found. </summary>
    public class RegistryKeyNotFoundException : Exception
    {
        public RegistryKeyNotFoundException()
        {
        }

        public RegistryKeyNotFoundException(string message) : base(message)
        {
        }
    }
}