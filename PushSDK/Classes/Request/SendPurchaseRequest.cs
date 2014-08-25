using Newtonsoft.Json;

namespace PushSDK.Classes
{
    [JsonObject]
    internal class SendPurchaseRequest : BaseRequest
    {
        public override string GetMethodName() { return "setPurchase"; }
    }
}
