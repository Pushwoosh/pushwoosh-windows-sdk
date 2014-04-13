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
    internal class RegistrationService
    {
        private RegistrationRequest _request;

        public event EventHandler SuccessefulyRegistered;
        public event EventHandler SuccessefulyUnregistered;

        public event CustomEventHandler<string> RegisterError;
        public event CustomEventHandler<string> UnregisterError;

        public void Register(string appID, string pushUri)
        {
            Debug.WriteLine("/********************************************************/");
            Debug.WriteLine("Register");

            _request.AppID = appID;
            _request.PushToken = pushUri;

            SendRequest(Constants.RegisterUrl, SuccessefulyRegistered, RegisterError);
        }

        public void Unregister()
        {
            Debug.WriteLine("/********************************************************/");
            Debug.WriteLine("Unregister");

            SendRequest(Constants.UnregisterUrl, SuccessefulyUnregistered, UnregisterError);
        }

        private async void SendRequest(Uri url, EventHandler successEvent, CustomEventHandler<string> errorEvent)
        {
            var webRequest = (HttpWebRequest)HttpWebRequest.Create(url);

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
                    if (successEvent != null)
                    {
                        successEvent(this, null);
                    }
                }
                else
                    errorMessage = JsonHelpers.GetStatusMessage(jRoot);

                if (!String.IsNullOrEmpty(errorMessage) && errorEvent != null)
                {
                    Debug.WriteLine("Error: " + errorMessage);
                    errorEvent(this, new CustomEventArgs<string> { Result = errorMessage });
                }
            }

            catch (Exception ex)
            {
                var errorMessage = ex.Message;
                Debug.WriteLine("Error: " + errorMessage);
                if (errorEvent != null)
                {
                    errorEvent(this, new CustomEventArgs<string> { Result = errorMessage });
                }
            }
        }
    }
}