using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Windows.Networking.PushNotifications;
using PushSDK.Classes;
using Windows.UI.Popups;
using Windows.UI.Core;
using Windows.UI;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.Xaml;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using System.Text.RegularExpressions;
namespace PushSDK
{
    public class NotificationService
    {
        #region private fields
        private readonly string _pushPage;

        private readonly Collection<Uri> _tileTrustedServers;

        private PushNotificationChannel _notificationChannel;

        private RegistrationService _registrationService;

        private string _pushContent;
        #endregion

        #region public properties

        /// <summary>
        /// Get content of last push notification
        /// </summary>
        public string LastPushContent
        {
            get
            {
                return _pushContent != null ? _pushContent : string.Empty;
            }
        }

        /// <summary>
        /// Get services for sending tags
        /// </summary>
        public TagsService Tags { get; private set; }

        /// <summary>
        /// Get user data from the last push came
        /// </summary>
        public string UserData { get{return LastPush != null ? LastPush.UserData : string.Empty;}}

        /// <summary>
        /// Get a service to manage Geozone
        /// </summary>
        public GeozoneService GeoZone { get; private set; }

        /// <summary>
        /// Get push token
        /// </summary>
        public string PushToken { get; private set; }
        #endregion

        #region internal properties

        private string AppID { get; set; }

        private StatisticService Statistic { get; set; }

        internal ToastPush LastPush { get; set; }

        #endregion

        #region public events

        /// <summary>
        /// User wants to see push
        /// </summary>
        public event CustomEventHandler<string> OnPushAccepted;

        /// <summary>
        /// On push token updated
        /// </summary>
        public event CustomEventHandler<Uri> OnPushTokenUpdated;
        #endregion

        #region Singleton

        private static NotificationService _instance;

        public static NotificationService GetCurrent(string appID, string pushPage, IEnumerable<string> tileTrustedServers)
        {
            return _instance ?? (_instance = tileTrustedServers == null ? new NotificationService(appID, pushPage) : new NotificationService(appID, pushPage, tileTrustedServers));
        }

        #endregion

        /// <param name="appID">PushWoosh application id</param>
        /// <param name="pushPage">Page on which the navigation is when receiving toast push notification </param>
        private NotificationService(string appID, string pushPage)
        {
            _pushPage = pushPage;
            AppID = appID;

            Statistic = new StatisticService(appID);
            Tags = new TagsService(appID);
            GeoZone = new GeozoneService(appID);
        }

        /// <param name="appID">PushWoosh application id</param>
        /// <param name="pushPage">Page on which the navigation is when receiving toast push notification </param>
        /// <param name="tileTrustedServers">Uris of trusted servers for tile images</param>
        private NotificationService(string appID, string pushPage, IEnumerable<string> tileTrustedServers)
            : this(appID, pushPage)
        {
            _tileTrustedServers = new Collection<Uri>(tileTrustedServers.Select(s => new Uri(s, UriKind.Absolute)).ToList());
        }

        #region public methods


        /// <summary>
        /// Creates push channel and regestrite it at pushwoosh server
        /// </summary>        
        public async void SubscribeToPushService()
        {
            
            try
            {
                _notificationChannel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
                if (_notificationChannel.Uri != null)
                    PushToken = _notificationChannel.Uri;

                Debug.WriteLine("Push Notification channel created successfully: " + PushToken);

                SubscribeToChannelEvents();
                SubscribeToService(AppID);
                
            }

            catch (Exception ex)
            {
                Debug.WriteLine("Exception occured while creating channel: " + ex.Message);
                // Could not create a channel. 
            }

        }

        /// <summary>
        /// Unsubscribe from pushes at pushwoosh server
        /// </summary>
        public void UnsubscribeFromPushes()
        {
            if (_registrationService == null) 
                return;
            _registrationService.Unregister();
        }

        #endregion

        #region private methods

        private void SubscribeToService(string appID)
        {
            if (_registrationService == null)
                _registrationService = new RegistrationService();

            _registrationService.Register(appID, _notificationChannel.Uri);
        }

        private void SubscribeToChannelEvents()
        {
            //Register to UriUpdated event - occurs when channel successfully opens
            _notificationChannel.PushNotificationReceived += ChannelShellToastNotificationReceived;

        }

        private void ChannelShellToastNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs e)
        {
            Debug.WriteLine("/********************************************************/");
            Debug.WriteLine("Incoming Notification: " + DateTime.Now.ToString());

            String notificationContent = String.Empty;

            String type = String.Empty;
            switch (e.NotificationType)
            {
                case PushNotificationType.Badge:
                    notificationContent = e.BadgeNotification.Content.GetXml();
                    type = "Badge";
                    break;

                case PushNotificationType.Tile:
                  
                    notificationContent = e.TileNotification.Content.GetXml();
                    type = "Tile";
                    break;

                case PushNotificationType.Toast:
             
                    notificationContent = e.ToastNotification.Content.GetXml();
                    type = "Toast";
                    break;

                case PushNotificationType.Raw:
                    notificationContent = e.RawNotification.Content;
                    type = "Raw";
                    break;
            }
            Debug.WriteLine("Received {0} notification", type);
            Debug.WriteLine("Notification content: " + notificationContent);

            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    try
                    {
                        _pushContent = notificationContent;
                         PushAccepted();
                        var alert = new MessageDialog("Notification content: " + notificationContent, type + " received");
                        alert.ShowAsync();

                     }
                    catch (Exception ex)
                    {
                        //Noting todo here
                    }
                });
             
            Debug.WriteLine("/********************************************************/");

 
        }

        private void PushAccepted()
        {
            if (OnPushAccepted != null)
                OnPushAccepted(this, new CustomEventArgs<string> {Result = LastPushContent});
        }
        
 
        #endregion
    }
}