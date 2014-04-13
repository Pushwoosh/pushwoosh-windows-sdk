using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushSDK
{
    internal static class Constants
    {
        // request consts
        public const int DeviceType = 8;
        private static string Host = "https://cp.pushwoosh.com/";

        private const string RegisterRequest = "registerDevice";
        private const string UnregisterRequest = "unregisterDevice";
        private const string StatisticRequest = "pushStat";
        private const string TagsRequest = "setTags";
        private const string GeozoneRequest = "getNearestZone";


        public static void setHost(string newHost)
        {
            Host=newHost;
        }
        public static string RequestDomain
        {
            get { return Host + "json/1.3/"; }
        }

        public static string HtmlPageUrl
        {
            get { return Host + "content/"; }
        }

        public static Uri RegisterUrl
        {
            get { return new Uri(RequestDomain + RegisterRequest, UriKind.Absolute); }
        }

        public static Uri UnregisterUrl
        {
            get { return new Uri(RequestDomain + UnregisterRequest, UriKind.Absolute); }
        }

        public static Uri StatisticUrl
        {
            get { return new Uri(RequestDomain + StatisticRequest, UriKind.Absolute); }
        }

        public static Uri TagsUrl
        {
            get { return new Uri(RequestDomain + TagsRequest, UriKind.Absolute); }
        }

        public static Uri GeozoneUrl
        {
            get { return new Uri(RequestDomain + GeozoneRequest, UriKind.Absolute); }
        }
    }
}
