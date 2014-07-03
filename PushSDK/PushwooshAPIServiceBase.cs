using System;
using System.Diagnostics;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PushSDK.Classes;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;

namespace PushSDK
{
    internal abstract class PushwooshAPIServiceBase
    {
        protected async Task InternalSendRequestAsync(object request, Uri url, EventHandler<JObject> successEvent, EventHandler<string> errorEvent)
        {
            HttpClient httpClient = new HttpClient();
            string requestString = String.Format("{{ \"request\":{0}}}", JsonConvert.SerializeObject(request));
            HttpContent httpContent = new StringContent(requestString, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            string webResponse = null;
            try
            {
                HttpResponseMessage response = await httpClient.PostAsync(url, httpContent);
                if (response.IsSuccessStatusCode)
                {
                    byte[] responseBytes = await response.Content.ReadAsByteArrayAsync();
                    webResponse = new string(System.Text.Encoding.UTF8.GetChars(responseBytes));
                    // haven't tested this, but should work as well: 
                    // webResponse = await response.Content.ReadAsStringAsync();
                }

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