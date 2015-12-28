using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using PushSDK;
using System.Text;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Windows.Networking.PushNotifications;
using System.Diagnostics;
using PushSDK.Classes;
using Newtonsoft.Json;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PushWooshSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private NotificationService service;

        public MainPage()
        {
            this.InitializeComponent();
        }

        void service_OnPushAccepted(object sender, ToastPush push)
        {
            string pushString = JsonConvert.SerializeObject(push);
            var alert = new MessageDialog("Notification content: " + pushString + " received");
            alert.ShowAsync();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {

        }


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (service != null)
            {
                service.StartGeoLocation();
            }

        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (service != null)
            {
                service.StopGeoLocation();
            }
        }

        private void UnSubButton_Click(object sender, RoutedEventArgs e)
        {
            if (service != null)
            {
                service.UnsubscribeFromPushes(
                    (obj, args) =>
                    {
                        MessageDialog dialog = new MessageDialog("Unsubscribed");
                        dialog.ShowAsync();
                    },
                    (obj, args) =>
                    {
                        MessageDialog dialog = new MessageDialog("Failed to unsubscribe");
                        dialog.ShowAsync();
                    });
            }
            SubButton.IsEnabled = true; ;
            UnSubButton.IsEnabled = false;
            tbPushToken.Text = "";
        }

        private void btnSendTag_Click(object sender, RoutedEventArgs e)
        {
            object value;

            if (tbTagValue.Text.IndexOf(',') != -1)
                value = tbTagValue.Text.Replace(", ", ",").Split(',');
            else
                value = tbTagValue.Text;

            object[] Values = new object[] { value };
            string[] Keys = new string[] { tbTagTitle.Text };

            if (service != null)
            {
                service.GetTags(
                    (obj, args) =>
                    {
                        MessageDialog dialog = new MessageDialog("Tags: " + args);
                        dialog.ShowAsync();
                    },
                    (obj, args) =>
                    {
                        MessageDialog dialog = new MessageDialog("Get tags failed");
                        dialog.ShowAsync();
                    });

                service.SendTag(Keys, Values,
                    (obj, args) =>
                    {
                        MessageDialog dialog = new MessageDialog("Tags sent succesfully");
                        dialog.ShowAsync();
                    },
                    (obj, args) =>
                    {
                        MessageDialog dialog = new MessageDialog("Tags sent failed");
                        dialog.ShowAsync();
                    });
            }
        }

        private void Subscribe_Tapped(object sender_, TappedRoutedEventArgs e)
        {
            try
            {

                string _PWId = PWID.Text;
                service = PushSDK.NotificationService.GetCurrent(_PWId);
                if (Host.Text.EndsWith("/"))
                {
                    service.SetHost(Host.Text);
                }
                else
                {
                    service.SetHost(Host.Text + "/");
                }

                service.OnPushAccepted += service_OnPushAccepted;
                service.OnPushTokenReceived += (sender, pushToken) => {tbPushToken.Text = pushToken;};
                service.OnPushTokenFailed += (sender, errorMessage) => { tbPushToken.Text = errorMessage; };
                service.SubscribeToPushService();

                if (service.PushToken != null)
                {
                    tbPushToken.Text = service.PushToken;
                }

                SubButton.IsEnabled = false;
                UnSubButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageDialog dialog = new MessageDialog("Host does not exist: \n" + ex.Message);
                dialog.ShowAsync();
            }

        }


    }
}
