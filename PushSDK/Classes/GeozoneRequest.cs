using Newtonsoft.Json;

namespace PushSDK.Classes
{
    [JsonObject]
    public class GeozoneRequest
    {
        [JsonProperty("application")]
        public string AppId { get; set; }

        [JsonProperty("hwid")]
        public string HardwareId
        {
            get { return SDKHelpers.GetDeviceUniqueId(); }
        }

        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("lng")]
        public double Lon { get; set; }
    }
}
