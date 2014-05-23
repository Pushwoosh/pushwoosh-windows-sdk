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

    internal class TagsService : PushwooshAPIServiceBase
    {
        private readonly string _appId;

        public event EventHandler<List<KeyValuePair<string, string> > > OnSuccess;
        public event EventHandler<string> OnError;

        public TagsService(string appId)
        {
            _appId = appId;
        }

        /// <summary>
        /// Sending tag to server
        /// </summary>
        /// <param name="tagList">Tags list</param>
        public async void SendRequest(List<KeyValuePair<string,object>> tagList)
        {
            JObject requestJson = BuildRequest(tagList);
            await InternalSendRequestAsync(requestJson, Constants.TagsUrl, (sender, arg) => { UploadStringCompleted(arg); }, OnError);
        }

        /// <summary>
        /// Sending tag to server
        /// </summary>
        /// <param name="tagList">Tags list</param>
        public async void SendRequest(String[] key, object[] values)
        {
            JObject requestJson = BuildRequest(key, values);
            await InternalSendRequestAsync(requestJson, Constants.TagsUrl, (sender, arg) => { UploadStringCompleted(arg); }, OnError);
        }

        private JObject BuildRequest(IEnumerable<KeyValuePair<string, object>> tagList)
        {
            JObject tags = new JObject();
            foreach (var tag in tagList)
            {
                tags.Add(new JProperty(tag.Key, tag.Value));
            }

            return BuildRequest(tags);
        }

        private JObject BuildRequest(String[] key, object[] values)
        {
            JObject tags = new JObject();

            int lenght = key.Length >= values.Length ? values.Length : key.Length;
            for (int i = 0; i < lenght; i++)
            {
                tags.Add(new JProperty(key[i], values[i]));
            }
            return BuildRequest(tags);
        }

        private JObject BuildRequest(JObject tags)
        {
            return new JObject(
                        new JProperty("application", _appId),
                        new JProperty("hwid", SDKHelpers.GetDeviceUniqueId()),
                        new JProperty("tags", tags));
        }

        private void UploadStringCompleted(JObject jRoot)
        {
            if (JsonHelpers.GetStatusCode(jRoot) == 200)
            {
                var skippedTags = new List<KeyValuePair<string, string>>();

                if (jRoot["response"].HasValues)
                {
                    JArray jItems = jRoot["response"]["skipped"] as JArray;
                    skippedTags = jItems.Select(jItem => new KeyValuePair<string, string>(jItem.Value<string>("tag"), jItem.Value<string>("reason"))).ToList();
                }

                if(OnSuccess != null)
                    OnSuccess(this, skippedTags);
            }
            else
            {
                if(OnError != null)
                    OnError(this, JsonHelpers.GetStatusMessage(jRoot));
            }
        }
    }

}
