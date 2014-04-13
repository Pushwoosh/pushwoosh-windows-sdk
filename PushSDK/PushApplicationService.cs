using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PushSDK
{
    internal class PushApplicationService
    {
        public PushApplicationService()
        {
            TileTrustedServers = new List<string>();

        }

        public PushApplicationService(string _PWAppId, string _PushPage, List<string> _TileTrustedServers, string _ServiceName)
        {
            this.PWAppId = _PWAppId;
            this.PushPage = _PushPage;
            this.TileTrustedServers = _TileTrustedServers;
            this.ServiceName = _ServiceName;
        }

        public NotificationService NotificationService
        {
            get
            {
                try
                {
                    if (string.IsNullOrEmpty(PWAppId) || PWAppId == "")
                        throw new ArgumentNullException("PWAppId");

                    return NotificationService.GetCurrent(PWAppId, PushPage, TileTrustedServers);
                }
                catch
                {
                    return null;
                }
            }

        }

        /// <summary>
        /// [Required] Get or set PushWoosh application id
        /// </summary>
        public string PWAppId { get; set; }

        /// <summary>
        /// [Optional] Page on which the navigation is when receiving toast push notification
        /// </summary>
        public string PushPage { get; set; }

        /// <summary>
        /// [Optional] Get or set trusted servers for receive tile notification
        /// </summary>
        public List<string> TileTrustedServers { get; set; }

        /// <summary>
        /// [Optional] The name that the web service uses to associate itself with the Push Notification Service.
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// [Default=false] Enable geo zone function for Push Woosh
        /// </summary>
        public bool GeoZones { get; set; }

        public void Subscribe()
        {
            NotificationService.SubscribeToPushService();
        }
    }
}