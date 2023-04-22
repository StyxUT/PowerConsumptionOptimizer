using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PowerProduction.Test")]
namespace PowerProduction
{

    public class ProductionStats
    {
        /// <summary>
        /// Current power production, in Watts. For historical requests, returns 0.
        /// </summary>
        [JsonProperty("current_power")]
        public int CurrentPower { get; set; }
        [JsonProperty("last_interval_end_at")]
        public int LastIntervalEndAt { get; set; }
        [JsonProperty("last_report_at")]
        public int LastReportAt { get; set; }
        /// <summary>
        /// Effective date of the response. For historical requests, returns the date requested. For current requests, returns the current date. 
        /// The format is YYYY-mm-dd unless you pass a datetime_format parameter.
        /// </summary>
        public DateOnly SummaryDate { get; set; }
    }
    public class ConsumptionStats
    {
        [JsonProperty("system_id")]
        public int? SystemId { get; set; }
        [JsonProperty("total_devices")]
        public int? TotalDevices { get; set; }
        [JsonProperty("intervals")]
        public List<Intervals>? Intervals { get; set; }
    }
    public class Intervals
    {
        [JsonProperty("end_at")]
        public int? EndAt { get; private set; }
        [JsonProperty("enwh")]
        public int? Enwh { get; private set; }
        [JsonProperty("devices_reporting")]
        public int? DevicesReporting { get; private set; }

    }

    public class Enphase : IPowerProduction
    {
        public class OauthInfo
        {
            /// <summary>
            ///  access_token is valid for ‘1 day’
            /// </summary>
            [JsonProperty("access_token")]
            public string AccessToken { get; set; } 
            /// <summary>
            /// refresh_token is valid for ‘1 week’
            /// </summary>
            [JsonProperty("token_type")]
            public string TokenType { get; set; }
            [JsonProperty("refresh_token")]
            public string RefreshToken { get; set; } 
            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }
            [JsonProperty("scope")]
            public string Scope { get; set; }
            [JsonProperty("enl_uid")]
            public string EnlUid { get; set; }
            [JsonProperty("enl_cid")]
            public string EnlCid { get; set; }
            [JsonProperty("enl_password_last")]
            public string EnlPasswordLastChanged { get; set; }
            [JsonProperty("is_internal_app")]
            public bool IsInternalApp { get; set; }
            [JsonProperty("app_type")]
            public string AppType { get; set; }
            [JsonProperty("jti")]
            public string Jti { get; set; }
        }

        private static readonly HttpClient client = new HttpClient();

        private static OauthInfo oauthInfo;

        private string OauthBaseURL { get; set; } = "https://api.enphaseenergy.com/oauth";
        public string OauthCode { get; set; }
        private string BaseURL { get; set; } = @"https://api.enphaseenergy.com";
        private string APIVersion { get; set; } = "v4";
        private string Method { get; set; } = "systems";
        public string SystemId { get; set; }
        private string APIKey { get; set; }
        private string ClientId { get; set; }
        private string ClientSecret { get; set; }
        private string UserId { get; set; }

        public Enphase()
        {
            throw new NotFiniteNumberException();

            // 1. check for access token
            // 2. if no access token, get authorization code
            // 3. if access token has expired, use refresh token
            // 4. if refresh token has expired, get authorization code
            // 5. use access token to make API calls

            //var result = GetClientAuthorizationCodeAsync().Result; // not implemented

#if DEBUG
            oauthInfo = new OauthInfo();
#else
           oauthInfo = GetTokensAsync().Result;
#endif
            client.BaseAddress = new Uri(BaseURL);
        }

        public Enphase(string apiKey, string oauthCode, string clientId)
        {
            APIKey = apiKey;
            OauthCode = oauthCode;
            ClientId = clientId;
        }

        /// <summary>
        /// Gets a client authorization code
        /// </summary>
        /// <returns>TBD</returns>
        private async Task<bool> GetClientAuthorizationCodeAsync()
        {

            throw new NotImplementedException();

            // Note: use the url below to 
            // https://api.enphaseenergy.com/oauth/authorize?response_type=code&client_id=ed5b6d41070340ed8921763741280984&redirect_uri=https://api.enphaseenergy.com/oauth/redirect_uri
            const string login = "";
            const string password = ""; // TODO: pull this information from a secure location

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "blah");

            // add content for POST
            HttpContent content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("response_type", "code"),
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("redirect_uri", $"{OauthBaseURL}/redirect_uri")
            });

            HttpResponseMessage response = await client.PostAsync($"{OauthBaseURL}/authorize", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return true;
            }
            else
            {
                Console.WriteLine($"!{System.Reflection.MethodBase.GetCurrentMethod()} Unsuccessful...");
                Console.WriteLine($"ResponseCode: {response.StatusCode}");
                Console.WriteLine($"ReasonPhrase: {response.ReasonPhrase}");
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"{result}");
                return false;
            }

        }

        /// <summary>
        /// Gets access and refresh token based on client authorization code
        /// </summary>
        /// <returns>TBD</returns>
        private async Task<OauthInfo?> GetTokensAsync()
        {
            var authValue = Base64Encode($"{ClientId}:{ClientSecret}");

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

            // add content for POST
            HttpContent content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("code", OauthCode),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("redirect_uri", $"{OauthBaseURL}/redirect_uri")
            });

            HttpResponseMessage response = await client.PostAsync($"{OauthBaseURL}/token", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<OauthInfo>(result);

            }
            else
            {
                Console.WriteLine($"!{System.Reflection.MethodBase.GetCurrentMethod()} Unsuccessful...");
                Console.WriteLine($"ResponseCode: {response.StatusCode}");
                Console.WriteLine($"ReasonPhrase: {response.ReasonPhrase}");
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"{result}");
                return null;
            }

        }

        /// <summary>
        /// Gets now access and refresh token based on Client ID, Client Secret, and refresh_token
        /// </summary>
        /// <returns>TBD</returns>
        private async Task<OauthInfo?> GetNewTokensAsync()
        {
            var authValue = Base64Encode($"{ClientId}:{ClientSecret}");

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

            // add content for POST
            var contentData = new Dictionary<string, string>
            {
                {"code", OauthCode},
                {"grant_type", "refresh_token"},
                {"refresh_token", $"unique_refresh_token"}
            };
            HttpContent content = new FormUrlEncodedContent(contentData);

            HttpResponseMessage response = await client.PostAsync($"{OauthBaseURL}/token", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<OauthInfo>(result);

            }
            else
            {
                Console.WriteLine($"!{System.Reflection.MethodBase.GetCurrentMethod()} Unsuccessful...");
                Console.WriteLine($"ResponseCode: {response.StatusCode}");
                Console.WriteLine($"ReasonPhrase: {response.ReasonPhrase}");
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"{result}");
                return null;
            }
        }

        public double? GetNetPowerProduction()
        {

            Console.WriteLine($"[{System.DateTime.Now}] Enphase - GetNetPowerProduction");
            //int? prod = GetProductionStatsAsync().Result.CurrentPower;
            int? prod = 1;
            int? cons = GetLastEnwh(GetConsumptionsStatsAsync().Result.Intervals);

            return (prod - cons);
        }

        private async Task<ProductionStats>? GetProductionStatsAsync()
        {
            var authValue = Base64Encode($"{ClientId}:{ClientSecret}");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("Applicaiton/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

            var path = $"api/{APIVersion}/{Method}/{SystemId}/summary?key={APIKey}";
            HttpResponseMessage response = await client.GetAsync(path);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ProductionStats>(result);
            }
            else
            {
                Console.WriteLine($"!{System.Reflection.MethodBase.GetCurrentMethod()} Unsuccessful...");
                Console.WriteLine($"ResponseCode: {response.StatusCode}");
                Console.WriteLine($"ReasonPhrase: {response.ReasonPhrase}");
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"{result}");
                return null;
            }
        }

        private async Task<ConsumptionStats>? GetConsumptionsStatsAsync()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("Applicaiton/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(oauthInfo.TokenType, oauthInfo.AccessToken);

            //var path = $"api/{APIVersion}/{Method}/{SystemId}/summary?key={APIKey}";
            var path = $"api/{APIVersion}/{Method}/{SystemId}/consumption_stats?key={APIKey}";
            HttpResponseMessage response = await client.GetAsync(path);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ConsumptionStats>(result);
            }
            else
            {
                Console.WriteLine($"!{System.Reflection.MethodBase.GetCurrentMethod()} Unsuccessful...");
                Console.WriteLine($"ResponseCode: {response.StatusCode}");
                Console.WriteLine($"ReasonPhrase: {response.ReasonPhrase}");
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"{result}");
                return null;
            }
        }

        private async Task<ConsumptionStats>? GetConsumptionsStatsV2Async()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("Applicaiton/json"));

            //var path = $"/api/v2/{Method}/{SystemId}/consumption_stats?key={APIKey}";
            var path = $"/api/v2/{Method}/{SystemId}/stats?key={APIKey}&user_id={UserId}";

            HttpResponseMessage response = await client.GetAsync(path);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ConsumptionStats>(result);
            }
            else
            {
                Console.WriteLine("!Request Unsuccessful...");
                Console.WriteLine($"ResponseCode: {response.StatusCode}");
                Console.WriteLine($"ReasonPhrase: {response.ReasonPhrase}");
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"{result}");
                return null;
            }
        }

        private int? GetLastEnwh(List<Intervals> intervals)
        {
            return intervals.Last<Intervals>().Enwh;
        }

        /// <summary>
        /// Takes as string and returns a base 64 encoded version of that string
        /// </summary>
        /// <param name="plainText">input string</param>
        /// <returns>base 64 encoded string</returns>
        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

    }
}
