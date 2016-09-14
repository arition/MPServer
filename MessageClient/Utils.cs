using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android.Util;

namespace MessageClient
{
    public static class Utils
    {
        public static HttpClient HttpClient { get; } = new HttpClient();
        private static Regex HashRegex { get; } = new Regex("(?<=\"Hash\".+?\").*?(?=\")", RegexOptions.Compiled);

        public static DateTime FromUnixTime(long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(unixTime);
        }

        public static long ToUnixTime(this DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date - epoch).TotalMilliseconds);
        }

        public static async Task LoginAsync()
        {
            var result = await HttpClient.PostAsync(
                "http://128.199.195.164:8080/api/Login",
                new StringContent("{" +
                                  "\"Username\":\"arition\"," +
                                  "\"PasswordHash\":\"GfSLc/sJJyPu7L6F5Jv4sIduosNg498i39KOGqQzI4w=\"" +
                                  "}", Encoding.UTF8, "application/json"));
            try
            {
                using (result)
                {
                    result.EnsureSuccessStatusCode();
                    var hash = HashRegex.Match(await result.Content.ReadAsStringAsync()).Value;
                    HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("KokoroHash", hash);
                }
            }
            catch (Exception ex)
            {
                Log.Error("KokoroGate", "Login: " + ex.Message);
            }
        }
    }
}