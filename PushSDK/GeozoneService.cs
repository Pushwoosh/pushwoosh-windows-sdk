using PushSDK.Classes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Windows.Devices.Geolocation;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Diagnostics;
using Windows.UI.Popups;

namespace PushSDK
{
    internal class GeozoneService
    {
        private const int MovementThreshold = 100;
        private readonly TimeSpan _minSendTime = TimeSpan.FromMinutes(10);

        private Geolocator _watcher;
        private Geolocator LazyWatcher
        {
            get
            {
                if (_watcher == null)
                {
                    _watcher = new Geolocator();
                }
                return _watcher;
            }
        }

        private readonly GeozoneRequest _geozoneRequest = new GeozoneRequest();

        public event EventHandler<CustomEventArgs<string>> OnError;

        private TimeSpan _lastTimeSend;

        public GeozoneService(string appId)
        {
            _geozoneRequest.AppId = appId;

            LazyWatcher.MovementThreshold = MovementThreshold;
        }


        public async void Start()
        {
            LazyWatcher.PositionChanged += WatcherOnPositionChanged;
            await LazyWatcher.GetGeopositionAsync(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }

        public void Stop()
        {
            LazyWatcher.PositionChanged -= WatcherOnPositionChanged;
        }


        private async void WatcherOnPositionChanged(Geolocator sender, PositionChangedEventArgs e)
        {
            try
            {
                if (DateTime.Now.TimeOfDay.Subtract(_lastTimeSend) >= _minSendTime)
                {
                    _geozoneRequest.Lat = e.Position.Coordinate.Latitude;
                    _geozoneRequest.Lon = e.Position.Coordinate.Longitude;

                    var webRequest = (HttpWebRequest)HttpWebRequest.Create(Constants.GeozoneUrl);

                    webRequest.Method = "POST";
                    webRequest.ContentType = "application/x-www-form-urlencoded";
                    string request = String.Format("{{ \"request\":{0}}}", JsonConvert.SerializeObject(_geozoneRequest));

                    byte[] requestBytes = System.Text.Encoding.UTF8.GetBytes(request);

                    try
                    {
                        // Write the channel URI to the request stream.
                        Stream requestStream = await webRequest.GetRequestStreamAsync();
                        requestStream.Write(requestBytes, 0, requestBytes.Length);

                        // Get the response from the server.
                        WebResponse response = await webRequest.GetResponseAsync();
                        StreamReader requestReader = new StreamReader(response.GetResponseStream());
                        String webResponse = requestReader.ReadToEnd();

                        string errorMessage = String.Empty;

                        Debug.WriteLine("Response: " + webResponse);

                        JObject jRoot = JObject.Parse(webResponse);
                        int code = JsonHelpers.GetStatusCode(jRoot);

                        if (JsonHelpers.GetStatusCode(jRoot) == 200)
                        {
                            double dist = jRoot["response"].Value<double>("distance");
                            if (dist > 0)
                                LazyWatcher.MovementThreshold = dist / 2;
                        }
                        else
                            errorMessage = JsonHelpers.GetStatusMessage(jRoot);

                        if (!String.IsNullOrEmpty(errorMessage) && OnError != null)
                        {
                            Debug.WriteLine("Error: " + errorMessage);
                            OnError(this, new CustomEventArgs<string> { Result = errorMessage });
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error: " + ex.Message);
                        if(OnError != null)
                        {
                            OnError(this, new CustomEventArgs<string> { Result = ex.Message });    
                        }
                    }

                    _lastTimeSend = DateTime.Now.TimeOfDay;
                }
            }

            catch { }
        }
    }
}
