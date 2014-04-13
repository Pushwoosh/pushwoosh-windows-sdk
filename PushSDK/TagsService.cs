using Newtonsoft.Json.Linq;
using PushSDK.Classes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace PushSDK
{

    internal class TagsService
    {
        private readonly string _appId;
        private static readonly char[] NewLineChars = Environment.NewLine.ToCharArray();

        private readonly HttpClient _httpClient = new HttpClient();

        public event EventHandler<CustomEventArgs<List<KeyValuePair<string, string>>>> OnSendingComplete;
        public event EventHandler<CustomEventArgs<string>> OnError;

        public TagsService(string appId)
        {
            _appId = appId;
        }

        /// <summary>
        /// Sending tag to server
        /// </summary>
        /// <param name="tagList">Tags list</param>
        public async void SendRequest(String[] key, object[] values)
        {
            var webRequest = (HttpWebRequest)HttpWebRequest.Create(Constants.TagsUrl);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";

            string request = BuildRequest(key, values);

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
                    UploadStringCompleted(webResponse);
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
                if (OnError != null)
                {
                    OnError(this, new CustomEventArgs<string> { Result = ex.Message });
                }
            }
        }


        /// <summary>
        /// Sending tag to server
        /// </summary>
        /// <param name="jTagList">tag format: [tagKey:tagValue]</param>
        public async void SendRequest(string jTagList)
        {
            var webRequest = (HttpWebRequest)HttpWebRequest.Create(Constants.TagsUrl);

            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";

            string request = JsonConvert.SerializeObject(BuildRequest(jTagList));

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
                    UploadStringCompleted(webResponse);
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
                if(OnError != null)
                {
                    OnError(this, new CustomEventArgs<string> { Result = ex.Message });
                }
            }
        }

        private string BuildRequest(String[] key, object[] values)
        {
            JObject tags = new JObject();

            int lenght = key.Length >= values.Length ? values.Length : key.Length;
            for (int i = 0; i < lenght; i++)
            {
                tags.Add(new JProperty(key[i], values[i]));
            }
            return BuildRequest(tags.ToString());
        }

        private string BuildRequest(string tags)
        {
            return (new JObject(
               new JProperty("request",
                             new JObject(
                                 new JProperty("application", _appId),
                                 new JProperty("hwid", SDKHelpers.GetDeviceUniqueId()),
                                 new JProperty("tags", JObject.Parse(tags)))))).ToString().Replace("\r\n", "");

        }

        private void UploadStringCompleted(string responseBodyAsText)
        {

            JObject jRoot = JObject.Parse(responseBodyAsText);
            if (JsonHelpers.GetStatusCode(jRoot) == 200)
            {
                var skippedTags = new List<KeyValuePair<string, string>>();

                if (jRoot["response"].HasValues)
                {
                    JArray jItems = jRoot["response"]["skipped"] as JArray;

                    skippedTags = jItems.Select(jItem => new KeyValuePair<string, string>(jItem.Value<string>("tag"), jItem.Value<string>("reason"))).ToList();
                }

                OnSendingComplete(this, new CustomEventArgs<List<KeyValuePair<string, string>>> { Result = skippedTags });
            }
            else
                OnError(this, new CustomEventArgs<string> { Result = JsonHelpers.GetStatusMessage(jRoot) });
        }
    }

}
