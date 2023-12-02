using Microsoft.Extensions.Logging;
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

        const string BaseURL = "http://envoy.home";
        private static readonly HttpClient client = new();
        readonly IAsyncPolicy retryOnException;

        /// <summary>
        /// Creates an instance of EnphaseLocal
        /// </summary>
        /// <param name="logger"></param>
        public EnphaseLocal(ILogger<EnphaseLocal> logger)
        {
            _logger = logger;

            retryOnException = Policy.Handle<Exception>()
                .WaitAndRetryAsync(retryCount: 10,
                    sleepDurationProvider: (retry) => TimeSpan.FromSeconds(5 * retry),
                    onRetry: (ex, waitTime, retryNum, context) => { Console.WriteLine($"\t retry {retryNum} for {context.OperationKey} due to {ex.Message} \n\t\t next retry in {waitTime}"); });
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
            StringBuilder stringBuilder = new();

            stringBuilder.AppendLine($"EnphaseLocal - GetNetPowerProduction");

            var netPowerProduction = GetMeterDataAsync().Result.Watts * -1; // multiply net consumption by -1 to convert to net production

            stringBuilder.Append($"\t Net Power Production: {netPowerProduction} watts");

            _logger.LogInformation(stringBuilder.ToString());
            return netPowerProduction;
        }

        internal async Task<NetConsumption>? GetMeterDataAsync()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("Applicaiton/json"));

            var path = $"{BaseURL}/production.json";
            HttpResponseMessage response = await retryOnException.ExecuteAsync(async action => await client.GetAsync(path), new Context($"{System.Reflection.MethodBase.GetCurrentMethod()}"));

            StringBuilder stringBuilder = new();

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return ParseNetConsumption(result);
            }
            else
            {
                stringBuilder.AppendLine($"!{System.Reflection.MethodBase.GetCurrentMethod()} Unsuccessful...");
                stringBuilder.AppendLine($"ResponseCode: {response.StatusCode}");
                stringBuilder.Append($"ReasonPhrase: {response.ReasonPhrase}");
                var result = await response.Content.ReadAsStringAsync();
                _logger.LogError($"{result}");
                return null;
            }
        }

        internal static NetConsumption? ParseNetConsumption(string? result)
        {
            var parsedObject = JObject.Parse(result);
            var netConsumption = parsedObject["consumption"][1].ToString();
            return JsonConvert.DeserializeObject<NetConsumption>(netConsumption);
        }
    }
}
