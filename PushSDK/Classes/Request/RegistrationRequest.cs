using System;
using System.Threading;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace PushSDK.Classes
{
    [JsonObject]
    internal class RegistrationRequest : BaseRequest
    {
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

        [JsonProperty("timezone")]
        public double Timezone
        {
            get { return TimeZoneInfo.Local.BaseUtcOffset.TotalSeconds; }
        }

        public override string GetMethodName() { return "registerDevice"; }
    }
}