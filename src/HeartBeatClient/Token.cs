using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace HeartBeatClient
{
    public class Token:IDisposable
    {
        public string TokenEndPoint { get; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string RefreshToken { get; private set; }

        private HttpClient HttpClient { get; } = new HttpClient();

        public Token(string tokenEndPoint,string username,string password)
        {
            TokenEndPoint = tokenEndPoint;
            Username = username;
            Password = password;
        }

        public async Task<string> GetAccessToken()
        {
            var response =
                await HttpClient.PostAsync(TokenEndPoint, new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"username", Username},
                    {"password", Password},
                    {"grant_type", "password"},
                    {"scope", "roles offline_access"}
                }));
            response.EnsureSuccessStatusCode();
            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
            RefreshToken = json["refresh_token"].ToString();
            return json["access_token"].ToString();
        }

        public async Task<string> GetAccessToken(bool useRefreshToken)
        {
            if (!useRefreshToken) return await GetAccessToken();
            if (string.IsNullOrEmpty(RefreshToken)) throw new ArgumentNullException(nameof(RefreshToken));
            var response =
                await HttpClient.PostAsync(TokenEndPoint, new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"grant_type", "refresh_token"},
                    {"refresh_token", RefreshToken}
                }));
            response.EnsureSuccessStatusCode();
            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
            RefreshToken = json["refresh_token"].ToString();
            return json["access_token"].ToString();
        }

        public void Dispose()
        {
            HttpClient.Dispose();
        }
    }
}
