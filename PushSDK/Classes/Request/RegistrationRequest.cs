using System;
using System.Threading;
using Newtonsoft.Json;
using System.Xml.Linq;
using Windows.Devices.Enumeration.Pnp;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.ApplicationModel;

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

        [JsonProperty("os_version")]
        public string OSVersion
        {
            get {
                Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation deviceInfo = new Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation();
                string os = deviceInfo.OperatingSystem;
                return os;
            }
        }

        private const string ModelNameKey = "System.Devices.ModelName";
        private const string RootContainer = "{00000000-0000-0000-FFFF-FFFFFFFFFFFF}";

        /// <summary>
        /// Get the name of the model of this computer.
        /// </summary>
        /// <example>Surface with Windows 8</example>
        /// <returns>The name of the model of this computer.</returns>
        public static async Task<string> GetDeviceModelAsync()
        {
            var rootContainer = await PnpObject.CreateFromIdAsync(PnpObjectType.DeviceContainer, RootContainer, new[] { ModelNameKey });
            return (string)rootContainer.Properties[ModelNameKey];
        }

        [JsonProperty("device_model")]
        public string DeviceModel
        {
            get {
                string model = GetDeviceModelAsync().Result;
                return model;
            }
        }

        [JsonProperty("app_version")]
        public string AppVersion
        {
            get
            {
                var version = Package.Current.Id.Version;
                return String.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            }
        }

        public override string GetMethodName() { return "registerDevice"; }
    }
}