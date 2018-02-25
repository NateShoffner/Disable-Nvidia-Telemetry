using System;
using System.Net;
using Newtonsoft.Json.Linq;

namespace DisableNvidiaTelemetry.Utilities
{
    internal class UpdaterUtilities
    {
        public static event EventHandler<UpdateResponseEventArgs> UpdateResponse;

        public static void UpdateCheck(object userToken)
        {
            var releasesUrl = new Uri("https://api.github.com/repos/NateShoffner/Disable-Nvidia-Telemetry/releases");

            using (var client = new WebClient {Proxy = null})
            {
                client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0.15063; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2950.0 Safari/537.36");
                client.DownloadStringCompleted += Client_DownloadStringCompleted;
                client.DownloadStringAsync(releasesUrl, userToken);
            }
        }

        private static void Client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
                try
                {
                    var j = JArray.Parse(e.Result);
                    var latestVersion = j[0]["tag_name"].ToString();
                    var url = j[0]["html_url"].ToString();

                    UpdateResponse?.Invoke(null, new UpdateResponseEventArgs(new Version(latestVersion), new Uri(url), e.UserState));
                }

                catch (Exception ex)
                {
                    UpdateResponse?.Invoke(null, new UpdateResponseEventArgs(ex, e.UserState));
                }

            else
                UpdateResponse?.Invoke(null, new UpdateResponseEventArgs(e.Error, e.UserState));
        }

        public class UpdateCheckException : Exception
        {
            public UpdateCheckException(string message) : base(message)
            {
            }
        }

        public class UpdateResponseEventArgs : EventArgs
        {
            public UpdateResponseEventArgs(Version latestVersion, Uri url, object userToken = null)
            {
                LatestVersion = latestVersion;
                Url = url;
                UserToken = userToken;
            }

            public UpdateResponseEventArgs(Exception error, object userToken = null)
            {
                Error = error;
                UserToken = userToken;
            }

            public Version LatestVersion { get; }

            public Exception Error { get; }

            public object UserToken { get; }

            public Uri Url { get; }
        }
    }
}