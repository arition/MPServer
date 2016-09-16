using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace MessageClient
{
    [Service]
    public class MessageMoniterService : Service
    {
        protected HttpClient HttpClient { get; } = new HttpClient();
        protected Token Token { get; set; }
        protected CancellationTokenSource CancellationTokenSource { get; set; }

        public override void OnCreate()
        {
            Log.Info("MessageClient", "ServiceStart");

            var mSmsReceiver = new SmsBroadcastReceiver();
            
            var tokenEndPoint = PreferenceManager.GetDefaultSharedPreferences(this).All["PrefTokenEndPoint"] as string;
            var messageEndPoint =
                PreferenceManager.GetDefaultSharedPreferences(this).All["PrefMessageEndPoint"] as string;
            var messageUsername =
                PreferenceManager.GetDefaultSharedPreferences(this).All["PrefMessageUsername"] as string;
            var messagePassword =
                PreferenceManager.GetDefaultSharedPreferences(this).All["PrefMessagePassword"] as string;

            mSmsReceiver.ReceiveMessage += async (sender, e) =>
            {
                Log.Info("MessageClient", $"Receive Message: {e.Message.Content}");

                try
                {
                    Token = new Token(tokenEndPoint, messageUsername, messagePassword);
                    HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                        await Token.GetAccessToken());
                    var content = JsonConvert.SerializeObject(e.Message, Formatting.Indented, new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });
                    using (var result = await HttpClient.PostAsync(messageEndPoint,
                        new StringContent(content.ToString(), Encoding.UTF8, "application/json")))
                    {
                        result.EnsureSuccessStatusCode();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("MessageClient", "Send: " + ex.Message);
                }
            };

            var smsfilter = new IntentFilter(SmsBroadcastReceiver.SmsReceived) {Priority = 2147483647};
            RegisterReceiver(mSmsReceiver, smsfilter);

            /*var messageMoniter = new MessageMoniter();
            messageMoniter.ReceiveMessage += (sender, e) =>
            {
                Log.Info("KokoroGate", $"Receive Message: {e.Message.Content}");
            };
            messageMoniter.MoniterMessage(ContentResolver);*/

            base.OnCreate();
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
    }
}