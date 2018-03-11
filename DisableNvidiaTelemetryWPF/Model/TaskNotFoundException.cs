using System;

namespace DisableNvidiaTelemetry.Model
{
    /// <summary>
    ///     Represents an exception where a secheduled task could not be found.
    /// </summary>
    public class TaskNotFoundException : Exception
    {
        public TaskNotFoundException()
        {
        }

        public TaskNotFoundException(string message) : base(message)
        {
        }
    }
}