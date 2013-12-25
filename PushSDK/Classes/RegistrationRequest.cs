using System;
using Newtonsoft.Json;
using System.IO;
using System.Threading;

namespace PushSDK.Classes
{
    [JsonObject]
    public struct RegistrationRequest
    {
        [JsonProperty("application")]
        public string AppID { get; set; }

        [JsonProperty("device_type")]
        public int DeviceType
        {
            get { return Constants.DeviceType; }
        }

        [JsonProperty("push_token")]
        public string PushToken { get; set; }

        [JsonProperty("language")]
        public string Language
        {
            get { return System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName; }        
        }


        //Note: to get a result requires ID_CAP_IDENTITY_DEVICE         
        // to be added to the capabilities of the WMAppManifest         
        // this will then warn users in marketplace  
        [JsonProperty("hwid")]
        public string HardwareId
        {
            get { return SDKHelpers.GetDeviceUniqueId(); }
        }

        [JsonProperty("timezone")]
        public double Timezone
        {
            get { return TimeZoneInfo.Local.BaseUtcOffset.TotalSeconds; }
        }
    }
}