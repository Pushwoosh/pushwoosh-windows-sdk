using Windows.UI.Xaml.Controls;
using PushSDK;

namespace PushWooshSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            NotificationService service = PushSDK.NotificationService.GetCurrent("DC533-F5DA4");

            service.OnPushReceived += (sender, pushNotification) =>
            {
                //code to handle push notification
                string pushString = pushNotification.ToString(); //will return json push payload
            };

            service.OnPushAccepted += (sender, pushNotification) =>
            {
                //code to handle push notification
                string pushString = pushNotification.ToString(); //will return json push payload
            };

            service.OnPushTokenReceived += (sender, pushToken) =>
            {
                //code to handle push token
            };

            service.OnPushTokenFailed += (sender, errorMessage) =>
            {
                //code to handle push subscription failure
            };

            service.SubscribeToPushService();
        }
    }
}
