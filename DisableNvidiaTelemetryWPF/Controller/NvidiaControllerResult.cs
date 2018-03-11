#region

using System;
using DisableNvidiaTelemetry.Model;

#endregion

namespace DisableNvidiaTelemetry.Controller
{
    public class NvidiaControllerResult<T> where T : ITelemetry
    {
        public NvidiaControllerResult(T item, Exception error = null)
        {
            Item = item;
            Error = error;
        }

        public bool Modified { get; set; }

        public Exception Error { get; }

        public T Item { get; }

        public string Name { get; set; }
    }
}