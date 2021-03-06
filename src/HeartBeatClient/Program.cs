﻿using System;
using System.Threading.Tasks;
using System.CommandLine;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json.Linq;

namespace HeartBeatClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string username = null;
            string password = null;
            string tokenEndPoint = null;
            string heartBeatEndPoint = null;
            string device = null;
            string configFile = null;

            ArgumentSyntax.Parse(args, t =>
            {
                t.DefineOption("un|username", ref username, "Login Username");
                t.DefineOption("pw|password", ref password, "Login Password");
                t.DefineOption("turl|tokenEndPoint", ref tokenEndPoint, "Token EndPoint");
                t.DefineOption("hurl|heartBeatEndPoint", ref heartBeatEndPoint, "HeartBeat EndPoint");
                t.DefineOption("dev|device", ref device, "DeviceName");
                t.DefineOption("c|config", ref configFile,
                    "Config File. The option in file will overwrite options provided by args.");

                if (string.IsNullOrEmpty(configFile))
                {
                    if (string.IsNullOrEmpty(username)) t.ReportError("username is required");
                    if (string.IsNullOrEmpty(password)) t.ReportError("password is required");
                    if (string.IsNullOrEmpty(device)) t.ReportError("device is required");
                    if (string.IsNullOrEmpty(tokenEndPoint))
                        t.ReportError("tokenEndPoint is required");
                    if (string.IsNullOrEmpty(heartBeatEndPoint))
                        t.ReportError("heartBeatEndPoint is required");
                }
            });

            if (!string.IsNullOrEmpty(configFile))
            {
                using (var jsonText = File.OpenText(configFile))
                {
                    var json = JObject.Parse(jsonText.ReadToEnd());
                    username = json["username"].ToString();
                    password = json["password"].ToString();
                    device = json["device"].ToString();
                    tokenEndPoint = json["token_end_point"].ToString();
                    heartBeatEndPoint = json["heart_beat_end_point"].ToString();
                }
            }

            var token = new Token(tokenEndPoint, username, password);

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token.GetAccessToken().Result);

            while (true)
            {
                try
                {
                    var response = httpClient.PostAsync(heartBeatEndPoint,
                        new StringContent("{\"device\":\"" + device + "\"}", Encoding.UTF8, "application/json")).Result;
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", token.GetAccessToken().Result);
                    }
                    else
                    {
                        response.EnsureSuccessStatusCode();
                    }
                    Task.Delay(10*1000).Wait();
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.ToString());
                }
            }
        }
    }
}
