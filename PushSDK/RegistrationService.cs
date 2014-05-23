using System;
using System.Diagnostics;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PushSDK.Classes;

namespace PushSDK
{
    internal class RegistrationService : PushwooshAPIServiceBase
    {
        private RegistrationRequest _request;

        public event EventHandler SuccessefulyRegistered;
        public event EventHandler SuccessefulyUnregistered;

        public event EventHandler<string> RegisterError;
        public event EventHandler<string> UnregisterError;

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

        private async void SendRequest(Uri url, EventHandler successEvent, EventHandler<string> errorEvent)
        {
            await InternalSendRequestAsync(_request, url, (sender, arg) => { successEvent(this, null); }, errorEvent);
        }
    }
}