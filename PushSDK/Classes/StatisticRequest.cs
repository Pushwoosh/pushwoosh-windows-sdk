using Newtonsoft.Json;

namespace PushSDK.Classes
{
   [JsonObject]
    internal class StatisticRequest
    {
        [JsonProperty("application")]
        public string AppId { get; set; }

        [JsonProperty("hwid")]
        public string HardwareId
        {
            get { return SDKHelpers.GetDeviceUniqueId(); }
        }
    }
}
