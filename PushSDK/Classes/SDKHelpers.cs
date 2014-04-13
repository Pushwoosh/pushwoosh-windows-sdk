using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace PushSDK.Classes
{
    public static class SDKHelpers
    {
        public static string GetDeviceUniqueId()
        {
            return MyToolkit.Environment.Machine.DeviceID;
        }

        internal static ToastPush ParsePushData(string url)
        {
            Dictionary<string, string> pushParams = ParseQueryString(Uri.UnescapeDataString(url));
            return new ToastPush
                       {
                           Contnet = pushParams.ContainsKey("content") ? pushParams["content"] : string.Empty,
                           Hash = pushParams.ContainsKey("p") ? pushParams["p"] : string.Empty,
                           HtmlId = pushParams.ContainsKey("h") ? Convert.ToInt32(pushParams["h"]) : -1,
                           Url = pushParams.ContainsKey("l") ? new Uri(pushParams["l"], UriKind.Absolute) : null,
                           UserData = pushParams.ContainsKey("u") ? pushParams["u"] : string.Empty
                       };
        }

        private static Dictionary<string,string> ParseQueryString(string s)
        {
            var list = new Dictionary<string, string>();
           
            // remove anything other than query string from url
            if (s.Contains("?"))
            {
                s = s.Substring(s.IndexOf('?') + 1);
            }

            foreach (string vp in Regex.Split(s, "&"))
            {
                string[] singlePair = Regex.Split(vp, "=");
                if (singlePair.Length == 2)
                    list[singlePair[0]] = singlePair[1];
                else
                    // only one key with no value specified in query string
                    list[singlePair[0]] = string.Empty;
            }
            return list;
        }
    }
}