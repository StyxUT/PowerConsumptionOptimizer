using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;


[assembly: InternalsVisibleTo("PowerProduction.Tests")]
namespace PowerProduction
{
    public class EnphaseLocal : IPowerProduction
    {
        private readonly ILogger<EnphaseLocal> _logger;
        private readonly string _envoyToken;

        const string BaseURL = "https://envoy.home";
        private string? SessionId;

        private HttpClient _client;
        readonly IAsyncPolicy retryOnException;

        /// <summary>
        /// Creates an instance of EnphaseLocal
        /// </summary>
        /// <param name="logger"></param>
        public EnphaseLocal(ILogger<EnphaseLocal> logger, string envoyToken)
        {
            _logger = logger;
            _envoyToken = envoyToken;

            retryOnException = Policy.Handle<Exception>()
                .WaitAndRetryAsync(retryCount: 10,
                    sleepDurationProvider: (retry) => TimeSpan.FromSeconds(5 * retry),
                    onRetry: (ex, waitTime, retryNum, context) => { Console.WriteLine($"\t retry {retryNum} for {context.OperationKey} due to {ex.Message} \n\t\t next retry in {waitTime}"); });

            // disable certificate warnings
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            _client = new HttpClient(handler);
        }

        public class NetConsumption
        {
            [JsonProperty("measurementType")]
            public string MeasurementType { get; set; }
            [JsonProperty("activeCount")]
            public int ActiveCount { get; set; }
            [JsonProperty("readingTime")]
            public int ReadingTime { get; set; }
            [JsonProperty("wNow")]
            public double Watts { get; set; }
        }

        public double? GetNetPowerProduction()
        {
            double? netPowerProduction = 0;

            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine($"EnphaseLocal - GetNetPowerProduction");
            try
            {
                netPowerProduction = retryOnException.ExecuteAsync(action => GetMeterDataAsync(), context: new Context("GetNetPowerProduction")).Result.Watts * -1; // multiply net consumption by -1 to convert to net production

                stringBuilder.Append($"\t Net Power Production: {netPowerProduction ?? 0} watts");

                _logger.LogInformation(stringBuilder.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogCritical("GetNetPowerProduction - critical failure");
                _logger.LogCritical(ex.Message.ToString());
                _logger.LogCritical(ex.InnerException.ToString());
            }
            
            return netPowerProduction ?? 0;
        }

        internal async Task<NetConsumption>? GetMeterDataAsync()
        {
            _logger.LogDebug($"EnphaseLocal - GetMeterDataAsync");

            if (SessionId is null)
            {
                await ReauthorizeAsync();
            }

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/javascript"));
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.01));
            _client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            _client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            _client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
            _client.DefaultRequestHeaders.Referrer = new Uri($"{BaseURL}/home");

            _client.DefaultRequestHeaders.Remove("sessionId"); // Remove previous value if any
            _client.DefaultRequestHeaders.Add("sessionId", SessionId);

            var path = $"{BaseURL}/production.json";

            try
            {
                HttpResponseMessage response = await retryOnException.ExecuteAsync(async action => await _client.GetAsync(path), new Context($"{System.Reflection.MethodBase.GetCurrentMethod()}"));

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    return ParseNetConsumption(result);
                }
                else
                {
                    StringBuilder stringBuilder = new();
                    stringBuilder.AppendLine($"GetMeterDataAsync - !{System.Reflection.MethodBase.GetCurrentMethod()} Unsuccessful...");
                    stringBuilder.AppendLine($"ResponseCode: {response.StatusCode}");
                    stringBuilder.Append($"ReasonPhrase: {response.ReasonPhrase}");
                    _logger.LogError(stringBuilder.ToString());

                    var result = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"{result}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message.ToString());
                _logger.LogCritical(ex.InnerException.ToString());
            }
            return null;
        }

        internal static NetConsumption? ParseNetConsumption(string? result)
        {
            var parsedObject = JObject.Parse(result);
            var netConsumption = parsedObject["consumption"][1].ToString();
            return JsonConvert.DeserializeObject<NetConsumption>(netConsumption);
        }

        internal async Task ReauthorizeAsync()
        {
            _logger.LogDebug($"EnphaseLocal - Reauthorize");

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Accept", "*/*");
            _client.DefaultRequestHeaders.Add("Referrer", $"{BaseURL}/home");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{_envoyToken}");

            var path = $"{BaseURL}/auth/check_jwt";

            try
            {
                HttpResponseMessage response = await retryOnException.ExecuteAsync(async action => await _client.GetAsync(path), new Context($"{System.Reflection.MethodBase.GetCurrentMethod()}"));

                if (response.IsSuccessStatusCode)
                {
                    // Extract the sessionId from the response headers
                    if (response.Headers.TryGetValues("sessionId", out var values))
                    {
                        SessionId = values.FirstOrDefault();
                    }
                }
                else
                {
                    StringBuilder stringBuilder = new();
                    stringBuilder.AppendLine($"ReauthorizeAsync - !{System.Reflection.MethodBase.GetCurrentMethod()} Unsuccessful...");
                    stringBuilder.AppendLine($"ResponseCode: {response.StatusCode}");
                    stringBuilder.Append($"ReasonPhrase: {response.ReasonPhrase}");
                    _logger.LogError(stringBuilder.ToString());
                    var result = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"{result}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message.ToString());
                _logger.LogCritical(ex.InnerException.ToString());
            }
        }
    }
}
