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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;


namespace PushSDK
{
    public sealed class NotificationService
    {
        #region private fields
        private readonly string _pushPage;

        private PushNotificationChannel _notificationChannel;
        private RegistrationService _registrationService;
        #endregion

        #region public properties

        /// <summary>
        /// Get user data from the last push
        /// </summary>
        public string UserData { get { return LastPush != null ? LastPush.UserData : string.Empty; } }

        /// <summary>
        /// Get push token
        /// </summary>
        public string PushToken { get; private set; }

        /// <summary>
        /// Get unique hardware ID, used in communication with Pushwoosh Remote API
        /// </summary>
        public string DeviceUniqueID { get { return SDKHelpers.GetDeviceUniqueId(); } }
        #endregion

        #region internal properties
        /// <summary>
        /// Get services for sending tags
        /// </summary>
        internal TagsService Tags { get; private set; }

        /// <summary>
        /// Get a service to manage Geozone
        /// </summary>
        internal GeozoneService GeoZone { get; private set; }

        private string AppID { get; set; }

        private StatisticService Statistic { get; set; }

        internal ToastPush LastPush { get; set; }

        #endregion

        #region public events

        /// <summary>
        /// User wants to see push
        /// </summary>
        public event EventHandler<PushNotificationReceivedEventArgs> OnPushAccepted;

        /// <summary>
        /// Push registration succeeded
        /// </summary>
        public event EventHandler<string> OnPushTokenReceived;

        /// <summary>
        /// Push registration failed
        /// </summary>
        public event EventHandler<string> OnPushTokenFailed;

        #endregion

        #region Singleton

        private static NotificationService _instance;

        public static NotificationService GetCurrent(string appID, string pushPage)
        {
            return _instance ?? (_instance = new NotificationService(appID, pushPage));
        }

        #endregion

        /// <param name="appID">PushWoosh application id</param>
        /// <param name="pushPage">Page on which the navigation is when receiving toast push notification </param>
        private NotificationService(string appID, string pushPage)
        {
            _pushPage = pushPage;
            AppID = appID;
            PushToken = "";

            Statistic = new StatisticService(appID);
            Tags = new TagsService(appID);
            GeoZone = new GeozoneService(appID);
        }

        #region public methods

        /// <summary>
        /// Creates push channel and regestrite it at pushwoosh server
        /// </summary>        
        public async void SubscribeToPushService()
        {
            //do nothing if already subscribed
            if (_notificationChannel != null)
                return;

            try
            {
                _notificationChannel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
                if (_notificationChannel.Uri != null)
                    PushToken = _notificationChannel.Uri;

                Debug.WriteLine("Push Notification channel created successfully: " + PushToken);

                //Register to UriUpdated event - occurs when channel successfully opens
                _notificationChannel.PushNotificationReceived += ChannelShellToastNotificationReceived;

                //Register device on Pushwoosh
                SubscribeToPushwoosh(AppID);
            }

            catch (Exception ex)
            {
                Debug.WriteLine("Exception occured while creating channel: " + ex.Message);
                // Could not create a channel. 

                if(OnPushTokenFailed != null)
                    OnPushTokenFailed(this, ex.Message);
            }
        }

        /// <summary>
        /// Unsubscribe from pushes at pushwoosh server
        /// </summary>
        public void UnsubscribeFromPushes()
        {
            if (_registrationService == null || _notificationChannel == null)
                return;

            PushToken = "";
            _notificationChannel.Close();
            _notificationChannel = null;
            _registrationService.Unregister();
        }

        /// <summary>
        ///  send Tag
        /// </summary>
        public void SendTag([ReadOnlyArray()] String[] key,[ReadOnlyArray()]  object[] values)
        {        
            Tags.SendRequest(key,values);
        }

         public void StartGeoLocation()
         {
             GeoZone.Start();
         }

         public void StopGeoLocation()
         {
             GeoZone.Stop();
         }

         public void SetHost(string host)
         {
             Constants.setHost(host);
         }
        #endregion

        #region private methods

        private void SendTokenToPushwooshSuccess(object sender, object args)
        {
            if (OnPushTokenReceived != null)
                OnPushTokenReceived(this, _notificationChannel.Uri);
        }

        private void SendTokenToPushwooshFailed(object sender, string message)
        {
            if (OnPushTokenFailed != null)
                OnPushTokenFailed(this, message);
        }

        private void SubscribeToPushwoosh(string appID)
        {
            if (_registrationService == null)
            {
                _registrationService = new RegistrationService();

                _registrationService.SuccessefulyRegistered += SendTokenToPushwooshSuccess;
                _registrationService.RegisterError += SendTokenToPushwooshFailed;
            }

            _registrationService.Register(appID, _notificationChannel.Uri);
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
                        PushAccepted(e);
                    }
                    catch (Exception)
                    {
                        //Noting todo here
                    }
                });

            Debug.WriteLine("/********************************************************/");


        }

        private void PushAccepted(PushNotificationReceivedEventArgs pushEvent)
        {
            if (OnPushAccepted != null)
                OnPushAccepted(this, pushEvent);
        }


        #endregion
    }
}