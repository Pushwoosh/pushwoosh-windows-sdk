using Newtonsoft.Json.Linq;

namespace PushSDK.Classes
{
    public static class JsonHelpers
    {
        public static int GetStatusCode(JObject jRoot)
        {
            return jRoot.Value<int>("status_code");
        }

        public static string GetStatusMessage(JObject jRoot)
        {
            return jRoot.Value<string>("status_message");
        }
    }
}
