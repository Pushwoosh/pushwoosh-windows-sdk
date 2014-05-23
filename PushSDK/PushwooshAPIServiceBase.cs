using System;
using System.Diagnostics;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PushSDK.Classes;
using System.IO;
using System.Threading.Tasks;

namespace PushSDK
{
    internal abstract class PushwooshAPIServiceBase
    {
        protected async Task InternalSendRequestAsync(object request, Uri url, EventHandler<JObject> successEvent, EventHandler<string> errorEvent)
        {
            var webRequest = (HttpWebRequest)HttpWebRequest.Create(url);

            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";

            string requestString = String.Format("{{ \"request\":{0}}}", JsonConvert.SerializeObject(request));
            byte[] requestBytes = System.Text.Encoding.UTF8.GetBytes(requestString);

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
                    if (successEvent != null)
                    {
                        successEvent(this, jRoot);
                    }
                }
                else
                {
                    errorMessage = JsonHelpers.GetStatusMessage(jRoot);
                }

                if (!String.IsNullOrEmpty(errorMessage))
                {
                    Debug.WriteLine("Error: " + errorMessage);
                    if (errorEvent != null)
                        errorEvent(this, errorMessage);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;
                Debug.WriteLine("Error: " + errorMessage);
                if (errorEvent != null)
                {
                    errorEvent(this, errorMessage);
                }
            }
        }
    }
}