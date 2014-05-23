using System;
using Newtonsoft.Json;

namespace PushSDK.Classes
{
    [JsonObject]
    internal class ToastPush
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("html")]
        public int HtmlId { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("userdata")]
        public string UserData { get; set; }
    }
}
