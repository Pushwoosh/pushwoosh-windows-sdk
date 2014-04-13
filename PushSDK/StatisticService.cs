using PushSDK.Classes;
using System;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace PushSDK
{
    internal class StatisticService
    {
        private readonly StatisticRequest _request;
        private readonly HttpClient _httpClient = new HttpClient();

        public event EventHandler<CustomEventArgs<string>> OnError;

        public StatisticService(string appId)
        {
            _request = new StatisticRequest { AppId = appId };
        }

        public async void SendRequest()
        {

            var webRequest = (HttpWebRequest)HttpWebRequest.Create(Constants.StatisticUrl);

            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            string request = String.Format("{{ \"request\":{0}}}", JsonConvert.SerializeObject(_request));

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
                if (code == 200 || code == 103)
                {

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
                var errorMessage = ex.Message;
                Debug.WriteLine("Error: " + errorMessage);
                if (OnError != null)
                {
                    OnError(this, new CustomEventArgs<string> { Result = errorMessage });
                }
            }


        }
    }
}
