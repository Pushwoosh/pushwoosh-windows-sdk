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
using Windows.Storage;
using Newtonsoft.Json.Linq;


namespace PushSDK
{
    public sealed class NotificationService
    {
        #region private fields

        private PushNotificationChannel _notificationChannel;
        #endregion

        #region public properties

        /// <summary>
        /// Get user data from the last push
        /// </summary>
        public string UserData { get; private set; }

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
        /// Get a service to manage Geozone
        /// </summary>
        internal GeozoneService GeoZone { get; private set; }

        private string AppID { get; set; }

		internal PushNotificationReceivedEventArgs LastPush { get; set; }
		private bool hasPushOnStart = false;

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

        // may return null if no instance present
        public static NotificationService GetCurrent()
        {
            return _instance;
        }

        public static NotificationService GetCurrent(string appID, string pushPage)
        {
            if(appID == null)
                appID = (String) ApplicationData.Current.LocalSettings.Values["com.pushwoosh.appid"];

            if (appID == null)
                return null;

            if (appID != (String)ApplicationData.Current.LocalSettings.Values["com.pushwoosh.appid"])
                _instance = null;

            return _instance ?? (_instance = new NotificationService(appID));
        }

        #endregion

        /// <param name="appID">PushWoosh application id</param>
        /// <param name="pushPage">Page on which the navigation is when receiving toast push notification </param>
        private NotificationService(string appID)
        {
            AppID = appID;
            ApplicationData.Current.LocalSettings.Values["com.pushwoosh.appid"] = appID;

            PushToken = "";

            GeoZone = new GeozoneService(appID);

            AppOpenRequest request = new AppOpenRequest { AppId = appID };
            PushwooshAPIServiceBase.InternalSendRequestAsync(request, null, null);
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

			if(OnPushAccepted != null && hasPushOnStart)
			{
				OnPushAccepted(this, null);
			}
        }

        /// <summary>
        /// Unsubscribe from pushes at pushwoosh server
        /// </summary>
        public void UnsubscribeFromPushes(EventHandler<string> success, EventHandler<string> failure)
        {
            if (_notificationChannel != null)
            {
                _notificationChannel.Close();
                _notificationChannel = null;
            }

            PushToken = "";
            UnregisterRequest request = new UnregisterRequest { AppId = AppID };
            PushwooshAPIServiceBase.InternalSendRequestAsync(request, (obj, arg) => { if (success != null) success(this, null); }, failure);
        }

        /// <summary>
        ///  send Tag
        /// </summary>
        public void SendTag([ReadOnlyArray()] String[] key, [ReadOnlyArray()]  object[] values, EventHandler<string> OnTagSendSuccess, EventHandler<string> OnError)
        {
            SetTagsRequest request = new SetTagsRequest { AppId = AppID };
            request.BuildTags(key, values);
            PushwooshAPIServiceBase.InternalSendRequestAsync(request, (obj, arg) => { if (OnTagSendSuccess != null) OnTagSendSuccess(this, null); }, OnError);
        }

        public void SendTag(IList<KeyValuePair<string, object>> tagList, EventHandler<string> OnTagSendSuccess, EventHandler<string> OnError)
        {
            SetTagsRequest request = new SetTagsRequest { AppId = AppID };
            request.BuildTags(tagList);
            PushwooshAPIServiceBase.InternalSendRequestAsync(request, (obj, arg) => { if (OnTagSendSuccess != null) OnTagSendSuccess(this, null); }, OnError);
        }

        public void GetTags(EventHandler<string> OnTagsSuccess, EventHandler<string> OnError)
        {
            GetTagsRequest request = new GetTagsRequest { AppId = AppID };
            PushwooshAPIServiceBase.InternalSendRequestAsync(request, (obj, arg) => { if(OnTagsSuccess != null) OnTagsSuccess(this, request.Tags.ToString()); }, OnError);
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

        public void HandleStartPushStat(string arguments)
        {
            if (arguments != null && arguments.Length != 0)
            {
                try
                {
                    //Sample to handle push custom data on start
                    String args = System.Net.WebUtility.UrlDecode(arguments);
                    JObject jRoot = JObject.Parse(args);
                    if(jRoot.GetValue("pushwoosh") != null)
                    {
						JObject pushwoosh = jRoot["pushwoosh"] as JObject;
						pushwoosh.Add("onStart", true);
						UserData = jRoot.ToString(Newtonsoft.Json.Formatting.None);
						hasPushOnStart = true;

                        StatisticRequest request = new StatisticRequest { AppId = AppID };
                        PushwooshAPIServiceBase.InternalSendRequestAsync(request, null, null);
                    }
                }
                catch { }
            }
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
            string token = _notificationChannel.Uri.ToString();
            RegistrationRequest request = new RegistrationRequest { AppId = appID, PushToken = token };

            PushwooshAPIServiceBase.InternalSendRequestAsync(request, SendTokenToPushwooshSuccess, SendTokenToPushwooshFailed);
        }

        private void ChannelShellToastNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs e)
        {
            Debug.WriteLine("/********************************************************/");
            Debug.WriteLine("Incoming Notification: " + DateTime.Now.ToString());

            StatisticRequest request = new StatisticRequest { AppId = AppID };
            PushwooshAPIServiceBase.InternalSendRequestAsync(request, null, null);

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
			LastPush = pushEvent;

			//get userdata
			if (pushEvent.NotificationType == PushNotificationType.Toast)
			{
				try
				{
					XmlDocument doc = new XmlDocument();
					doc.LoadXml(pushEvent.ToastNotification.Content.GetXml());
					XmlElement root = doc.DocumentElement;
					string customData = root.GetAttributeNode("launch").Value;
					UserData = System.Net.WebUtility.UrlDecode(customData);
				}
				catch
				{}
			}

            if (OnPushAccepted != null)
                OnPushAccepted(this, pushEvent);
        }


        #endregion
    }
}