using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Phone.Shell;
using PushSDK;

namespace PushWooshWP7Sample
{
    public partial class MainPage 
    {
        private readonly NotificationService _service = null;

        public MainPage()
        {
            InitializeComponent();

            _service = NotificationService.GetCurrent("3A43A-A3EAB", null, null);

            _service.OnPushTokenReceived += (sender, args) =>
            {
                tbPushToken.Text = args.ToString();
            };

            _service.OnPushTokenFailed += (sender, args) =>
            {
                tbPushToken.Text = args.ToString();
            };

            _service.OnPushAccepted += (sender, args) =>
            {
                tbPush.Text = args.ToString();
            };

            _service.SubscribeToPushService();

            //tbPushToken.Text = _service.PushToken;
            ResetMyMainTile();
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            _service.UnsubscribeFromPushes((obj, args) => { MessageBox.Show("Unsubscribed"); }, (obj, args) => { MessageBox.Show("Failed to unsubscribe"); });
        }

        private void btnSendTag_Click(object sender, RoutedEventArgs e)
        {
            _service.GetTags((obj, args) => { MessageBox.Show("Tags: " + args.ToString()); }, (obj, args) => { MessageBox.Show("Error: " + args.ToString()); });

            var tagsList = new List<KeyValuePair<string, object>>();

            object value;
            int iValue;
            if (int.TryParse(tbTagValue.Text, out iValue))
                value = iValue;
            else if (tbTagValue.Text.IndexOf(',') != -1)
                value = tbTagValue.Text.Replace(", ", ",").Split(',');
            else
                value = tbTagValue.Text;

            tagsList.Add(new KeyValuePair<string, object>(tbTagTitle.Text, value));

            _service.SendTag(tagsList,
                (obj, args) =>
                {
                    MessageBox.Show("Tag has been sent!");
                },
                (obj, args) => MessageBox.Show("Error while sending the tags: \n" + args.ToString())
            );

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _service.StartGeoLocation();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _service.StopGeoLocation();
        }

        private void ResetMyMainTile()
        {

            ShellTile tileToFind = ShellTile.ActiveTiles.First();
            if (tileToFind != null)
            {

                StandardTileData newTileData = new StandardTileData
                {
                    Title = "Push Woosh",
                    BackgroundImage = new Uri("Background.png", UriKind.RelativeOrAbsolute),
                    Count = 0,
                    BackTitle = "",
                    BackBackgroundImage = new Uri("doesntexist.png", UriKind.RelativeOrAbsolute),
                    BackContent = ""
                };

                tileToFind.Update(newTileData);
            }
        }
    }
}