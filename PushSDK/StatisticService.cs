using System.Net;
using Newtonsoft.Json;
using PushSDK.Classes;

namespace PushSDK
{
    internal class StatisticService : PushwooshAPIServiceBase
    {
        private readonly StatisticRequest _request;

        public StatisticService(string appId)
        {
            _request = new StatisticRequest { AppId = appId };
        }

        public async void SendPushOpen(string hash)
        {
            _request.Hash = hash;
            await InternalSendRequestAsync(_request, Constants.StatisticUrl, null, null);
        }

        public async void SendAppOpen()
        {
            _request.Hash = null;
            await InternalSendRequestAsync(_request, Constants.AppOpenUrl, null, null);
        }
    }
}
