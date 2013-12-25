using Newtonsoft.Json;

namespace PushSDK.Classes
{
    [JsonObject]
    public class StatisticRequest
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