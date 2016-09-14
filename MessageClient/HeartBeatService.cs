using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Util;

namespace MessageClient
{
    [Service]
    public class HeartBeatService : Service
    {
        protected HttpClient HttpClient { get; } = new HttpClient();
        protected Token Token { get; set; } 
        protected CancellationTokenSource CancellationTokenSource { get; set; }

        public override async void OnCreate()
        {
            base.OnCreate();
            var tokenEndPoint = PreferenceManager.GetDefaultSharedPreferences(this).All["PrefTokenEndPoint"] as string;
            var heartBeatEndPoint =
                PreferenceManager.GetDefaultSharedPreferences(this).All["PrefHeartBeatEndPoint"] as string;
            var heartBeatUsername =
                PreferenceManager.GetDefaultSharedPreferences(this).All["PrefHeartBeatUsername"] as string;
            var heartBeatPassword =
                PreferenceManager.GetDefaultSharedPreferences(this).All["PrefHeartBeatPassword"] as string;

            try
            {
                Token = new Token(tokenEndPoint, heartBeatUsername, heartBeatPassword);
                HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    await Token.GetAccessToken());
            }
            catch (Exception e)
            {
                Log.Error("MessageClient", e.ToString());
            }

            CancellationTokenSource = new CancellationTokenSource();

#pragma warning disable 4014
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var response = await HttpClient.PostAsync(heartBeatEndPoint,
                            new StringContent("{\"device\":\"" + Build.Model + "\"}", Encoding.UTF8,
                                "application/json"));
                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            HttpClient.DefaultRequestHeaders.Authorization =
                                new AuthenticationHeaderValue("Bearer", await Token.GetAccessToken());
                        }
                        else
                        {
                            response.EnsureSuccessStatusCode();
                            Log.Info("MessageClient", "Send HeartBeat Success");
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error("MessageClient", e.ToString());
                    }
                    await Task.Delay(10 * 1000);
                }
                // ReSharper disable once FunctionNeverReturns
            },CancellationTokenSource.Token);
#pragma warning restore 4014
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            CancellationTokenSource.Cancel();
            Token?.Dispose();
            HttpClient.Dispose();
        }
    }
}