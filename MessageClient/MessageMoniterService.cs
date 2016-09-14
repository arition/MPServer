using System;
using System.Net;
using System.Net.Http;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;

namespace MessageClient
{
    [Service]
    public class MessageMoniterService : Service
    {
        public override void OnCreate()
        {
            Log.Info("KokoroGate", "ServiceStart");

            var mSmsReceiver = new SmsBroadcastReceiver();
            mSmsReceiver.ReceiveMessage += async (sender, e) =>
            {
                Log.Info("KokoroGate", $"Receive Message: {e.Message.Content}");
                while (true)
                {
                    try
                    {
                        using (var result = await Utils.HttpClient.PostAsync("http://128.199.195.164:8080/api/Message",
                            new StringContent("", Encoding.UTF8, "application/json")))
                        {
                            if (result.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                await Utils.LoginAsync();
                                continue;
                            }
                            result.EnsureSuccessStatusCode();
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("KokoroGate", "Send: " + ex.Message);
                        break;
                    }
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