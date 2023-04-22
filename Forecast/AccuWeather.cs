using ConfigurationSettings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Contrib.WaitAndRetry;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;


[assembly: InternalsVisibleTo("Forecast.Tests")]
namespace Forecast
{
    public class AccuWeather : IForecast
    {
        private readonly ILogger<AccuWeather> _logger;
        private static AccuWeatherSettings accuWeatherSettings;
        private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;
        internal AccuWeatherForecast[] _forecast;

        private readonly string method = "hourly/12hour";
        private readonly string version = "v1";
        private readonly string baseURL = "http://dataservice.accuweather.com/forecasts";

        private static readonly HttpClient client = new();

        public AccuWeather(ILogger<AccuWeather> logger)
        {
             _logger = logger;
                
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                .AddJsonFile("forecast-appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"forecast-appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .AddUserSecrets(Assembly.GetExecutingAssembly(), true);

            
            _logger.LogTrace($"AccuWeather - Builder BasePath: { Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) }" );

            IConfigurationRoot build = builder.Build();

            accuWeatherSettings = new();
            build.GetSection("AccuWeather").Bind(accuWeatherSettings);

            _retryPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(x => x.StatusCode is >= HttpStatusCode.InternalServerError or HttpStatusCode.RequestTimeout)
                .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMicroseconds(2), 5));
        }

        /// <summary>
        /// Gets the projected solar Irradiance over the next hour
        /// </summary>
        /// <returns>projected solar irradiance</returns>
        public double GetSolarIrradianceNextHour()
        {
            _logger.LogDebug("AccuWeather - GetSolarIrradianceNextHour");

            ManageForecast();

            var currentDateTime = DateTime.Now;
            return _forecast.FirstOrDefault(forecast => forecast.DateTime > currentDateTime).SolarIrradiance.Value;
        }

        /// <summary>
        /// Gets the projected solar Irradiance over the next hour
        /// </summary>
        /// <returns>projected solar irradiance</returns>
        public List<double> GetSolarIrradianceByHour()
        {
            _logger.LogDebug("AccuWeather - GetSolarIrradianceByHour");
            ManageForecast();

            var currentDateTime = DateTime.Now;
            return _forecast
                .Where(forecast => forecast.DateTime > currentDateTime)
                .Select(forecast => forecast.SolarIrradiance.Value).ToList<double>();
        }

        internal async Task<AccuWeatherForecast[]> GetForecast()
        {
            StringBuilder stringBuilder = new();
            var path = $"{baseURL}/{version}/{method}/{accuWeatherSettings.Locationkey}?apikey={accuWeatherSettings.APIKey}&details=true";

            _logger.LogTrace($"AcccuWeather GET: {path}");

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("Applicaiton/json"));

            stringBuilder.Append($"AccuWeather - GetForecast");

# if DEBUG
            const string testJson = @"[{""DateTime"":""2023-04-22T10:00:00-06:00"",""EpochDateTime"":1682179200,""WeatherIcon"":7,""IconPhrase"":""Cloudy"",""HasPrecipitation"":false,""IsDaylight"":true,""Temperature"":{""Value"":45.0,""Unit"":""F"",""UnitType"":18},""RealFeelTemperature"":{""Value"":42.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""RealFeelTemperatureShade"":{""Value"":40.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""WetBulbTemperature"":{""Value"":38.0,""Unit"":""F"",""UnitType"":18},""DewPoint"":{""Value"":28.0,""Unit"":""F"",""UnitType"":18},""Wind"":{""Speed"":{""Value"":9.2,""Unit"":""mi/h"",""UnitType"":9},""Direction"":{""Degrees"":310,""Localized"":""NW"",""English"":""NW""}},""WindGust"":{""Speed"":{""Value"":10.4,""Unit"":""mi/h"",""UnitType"":9}},""RelativeHumidity"":52,""IndoorRelativeHumidity"":37,""Visibility"":{""Value"":10.0,""Unit"":""mi"",""UnitType"":2},""Ceiling"":{""Value"":6600.0,""Unit"":""ft"",""UnitType"":0},""UVIndex"":1,""UVIndexText"":""Low"",""PrecipitationProbability"":7,""ThunderstormProbability"":0,""RainProbability"":7,""SnowProbability"":0,""IceProbability"":0,""TotalLiquid"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Rain"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Snow"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Ice"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""CloudCover"":96,""Evapotranspiration"":{""Value"":0.01,""Unit"":""in"",""UnitType"":1},""SolarIrradiance"":{""Value"":254.6,""Unit"":""W/m²"",""UnitType"":33},""MobileLink"":"""",""Link"":""""},{""DateTime"":""2023-04-22T11:00:00-06:00"",""EpochDateTime"":1682182800,""WeatherIcon"":7,""IconPhrase"":""Cloudy"",""HasPrecipitation"":false,""IsDaylight"":true,""Temperature"":{""Value"":46.0,""Unit"":""F"",""UnitType"":18},""RealFeelTemperature"":{""Value"":43.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""RealFeelTemperatureShade"":{""Value"":40.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""WetBulbTemperature"":{""Value"":39.0,""Unit"":""F"",""UnitType"":18},""DewPoint"":{""Value"":29.0,""Unit"":""F"",""UnitType"":18},""Wind"":{""Speed"":{""Value"":9.2,""Unit"":""mi/h"",""UnitType"":9},""Direction"":{""Degrees"":311,""Localized"":""NW"",""English"":""NW""}},""WindGust"":{""Speed"":{""Value"":11.5,""Unit"":""mi/h"",""UnitType"":9}},""RelativeHumidity"":50,""IndoorRelativeHumidity"":38,""Visibility"":{""Value"":10.0,""Unit"":""mi"",""UnitType"":2},""Ceiling"":{""Value"":7100.0,""Unit"":""ft"",""UnitType"":0},""UVIndex"":2,""UVIndexText"":""Low"",""PrecipitationProbability"":7,""ThunderstormProbability"":0,""RainProbability"":7,""SnowProbability"":0,""IceProbability"":0,""TotalLiquid"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Rain"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Snow"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Ice"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""CloudCover"":93,""Evapotranspiration"":{""Value"":0.01,""Unit"":""in"",""UnitType"":1},""SolarIrradiance"":{""Value"":306.2,""Unit"":""W/m²"",""UnitType"":33},""MobileLink"":"""",""Link"":""""},{""DateTime"":""2023-04-22T12:00:00-06:00"",""EpochDateTime"":1682186400,""WeatherIcon"":7,""IconPhrase"":""Cloudy"",""HasPrecipitation"":false,""IsDaylight"":true,""Temperature"":{""Value"":47.0,""Unit"":""F"",""UnitType"":18},""RealFeelTemperature"":{""Value"":46.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""RealFeelTemperatureShade"":{""Value"":41.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""WetBulbTemperature"":{""Value"":40.0,""Unit"":""F"",""UnitType"":18},""DewPoint"":{""Value"":30.0,""Unit"":""F"",""UnitType"":18},""Wind"":{""Speed"":{""Value"":9.2,""Unit"":""mi/h"",""UnitType"":9},""Direction"":{""Degrees"":311,""Localized"":""NW"",""English"":""NW""}},""WindGust"":{""Speed"":{""Value"":11.5,""Unit"":""mi/h"",""UnitType"":9}},""RelativeHumidity"":51,""IndoorRelativeHumidity"":39,""Visibility"":{""Value"":10.0,""Unit"":""mi"",""UnitType"":2},""Ceiling"":{""Value"":7100.0,""Unit"":""ft"",""UnitType"":0},""UVIndex"":3,""UVIndexText"":""Moderate"",""PrecipitationProbability"":7,""ThunderstormProbability"":0,""RainProbability"":7,""SnowProbability"":0,""IceProbability"":0,""TotalLiquid"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Rain"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Snow"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Ice"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""CloudCover"":91,""Evapotranspiration"":{""Value"":0.01,""Unit"":""in"",""UnitType"":1},""SolarIrradiance"":{""Value"":340.2,""Unit"":""W/m²"",""UnitType"":33},""MobileLink"":"""",""Link"":""""},{""DateTime"":""2023-04-22T13:00:00-06:00"",""EpochDateTime"":1682190000,""WeatherIcon"":7,""IconPhrase"":""Cloudy"",""HasPrecipitation"":false,""IsDaylight"":true,""Temperature"":{""Value"":48.0,""Unit"":""F"",""UnitType"":18},""RealFeelTemperature"":{""Value"":47.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""RealFeelTemperatureShade"":{""Value"":43.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""WetBulbTemperature"":{""Value"":40.0,""Unit"":""F"",""UnitType"":18},""DewPoint"":{""Value"":30.0,""Unit"":""F"",""UnitType"":18},""Wind"":{""Speed"":{""Value"":9.2,""Unit"":""mi/h"",""UnitType"":9},""Direction"":{""Degrees"":312,""Localized"":""NW"",""English"":""NW""}},""WindGust"":{""Speed"":{""Value"":11.5,""Unit"":""mi/h"",""UnitType"":9}},""RelativeHumidity"":49,""IndoorRelativeHumidity"":39,""Visibility"":{""Value"":10.0,""Unit"":""mi"",""UnitType"":2},""Ceiling"":{""Value"":7100.0,""Unit"":""ft"",""UnitType"":0},""UVIndex"":3,""UVIndexText"":""Moderate"",""PrecipitationProbability"":7,""ThunderstormProbability"":0,""RainProbability"":7,""SnowProbability"":0,""IceProbability"":0,""TotalLiquid"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Rain"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Snow"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Ice"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""CloudCover"":93,""Evapotranspiration"":{""Value"":0.01,""Unit"":""in"",""UnitType"":1},""SolarIrradiance"":{""Value"":334.6,""Unit"":""W/m²"",""UnitType"":33},""MobileLink"":"""",""Link"":""""},{""DateTime"":""2023-04-22T14:00:00-06:00"",""EpochDateTime"":1682193600,""WeatherIcon"":7,""IconPhrase"":""Cloudy"",""HasPrecipitation"":false,""IsDaylight"":true,""Temperature"":{""Value"":49.0,""Unit"":""F"",""UnitType"":18},""RealFeelTemperature"":{""Value"":47.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""RealFeelTemperatureShade"":{""Value"":44.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""WetBulbTemperature"":{""Value"":41.0,""Unit"":""F"",""UnitType"":18},""DewPoint"":{""Value"":30.0,""Unit"":""F"",""UnitType"":18},""Wind"":{""Speed"":{""Value"":9.2,""Unit"":""mi/h"",""UnitType"":9},""Direction"":{""Degrees"":309,""Localized"":""NW"",""English"":""NW""}},""WindGust"":{""Speed"":{""Value"":11.5,""Unit"":""mi/h"",""UnitType"":9}},""RelativeHumidity"":48,""IndoorRelativeHumidity"":39,""Visibility"":{""Value"":10.0,""Unit"":""mi"",""UnitType"":2},""Ceiling"":{""Value"":7100.0,""Unit"":""ft"",""UnitType"":0},""UVIndex"":2,""UVIndexText"":""Low"",""PrecipitationProbability"":6,""ThunderstormProbability"":0,""RainProbability"":6,""SnowProbability"":0,""IceProbability"":0,""TotalLiquid"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Rain"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Snow"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Ice"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""CloudCover"":95,""Evapotranspiration"":{""Value"":0.01,""Unit"":""in"",""UnitType"":1},""SolarIrradiance"":{""Value"":314.5,""Unit"":""W/m²"",""UnitType"":33},""MobileLink"":"""",""Link"":""""},{""DateTime"":""2023-04-22T15:00:00-06:00"",""EpochDateTime"":1682197200,""WeatherIcon"":7,""IconPhrase"":""Cloudy"",""HasPrecipitation"":false,""IsDaylight"":true,""Temperature"":{""Value"":50.0,""Unit"":""F"",""UnitType"":18},""RealFeelTemperature"":{""Value"":48.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""RealFeelTemperatureShade"":{""Value"":45.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""WetBulbTemperature"":{""Value"":41.0,""Unit"":""F"",""UnitType"":18},""DewPoint"":{""Value"":30.0,""Unit"":""F"",""UnitType"":18},""Wind"":{""Speed"":{""Value"":9.2,""Unit"":""mi/h"",""UnitType"":9},""Direction"":{""Degrees"":289,""Localized"":""WNW"",""English"":""WNW""}},""WindGust"":{""Speed"":{""Value"":10.4,""Unit"":""mi/h"",""UnitType"":9}},""RelativeHumidity"":47,""IndoorRelativeHumidity"":39,""Visibility"":{""Value"":10.0,""Unit"":""mi"",""UnitType"":2},""Ceiling"":{""Value"":7100.0,""Unit"":""ft"",""UnitType"":0},""UVIndex"":2,""UVIndexText"":""Low"",""PrecipitationProbability"":6,""ThunderstormProbability"":0,""RainProbability"":6,""SnowProbability"":0,""IceProbability"":0,""TotalLiquid"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Rain"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Snow"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Ice"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""CloudCover"":96,""Evapotranspiration"":{""Value"":0.01,""Unit"":""in"",""UnitType"":1},""SolarIrradiance"":{""Value"":287.5,""Unit"":""W/m²"",""UnitType"":33},""MobileLink"":"""",""Link"":""""},{""DateTime"":""2023-04-22T16:00:00-06:00"",""EpochDateTime"":1682200800,""WeatherIcon"":7,""IconPhrase"":""Cloudy"",""HasPrecipitation"":false,""IsDaylight"":true,""Temperature"":{""Value"":51.0,""Unit"":""F"",""UnitType"":18},""RealFeelTemperature"":{""Value"":48.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""RealFeelTemperatureShade"":{""Value"":46.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""WetBulbTemperature"":{""Value"":42.0,""Unit"":""F"",""UnitType"":18},""DewPoint"":{""Value"":30.0,""Unit"":""F"",""UnitType"":18},""Wind"":{""Speed"":{""Value"":8.1,""Unit"":""mi/h"",""UnitType"":9},""Direction"":{""Degrees"":285,""Localized"":""WNW"",""English"":""WNW""}},""WindGust"":{""Speed"":{""Value"":10.4,""Unit"":""mi/h"",""UnitType"":9}},""RelativeHumidity"":45,""IndoorRelativeHumidity"":39,""Visibility"":{""Value"":10.0,""Unit"":""mi"",""UnitType"":2},""Ceiling"":{""Value"":7100.0,""Unit"":""ft"",""UnitType"":0},""UVIndex"":1,""UVIndexText"":""Low"",""PrecipitationProbability"":6,""ThunderstormProbability"":0,""RainProbability"":6,""SnowProbability"":0,""IceProbability"":0,""TotalLiquid"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Rain"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Snow"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Ice"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""CloudCover"":94,""Evapotranspiration"":{""Value"":0.01,""Unit"":""in"",""UnitType"":1},""SolarIrradiance"":{""Value"":263.5,""Unit"":""W/m²"",""UnitType"":33},""MobileLink"":"""",""Link"":""""},{""DateTime"":""2023-04-22T17:00:00-06:00"",""EpochDateTime"":1682204400,""WeatherIcon"":7,""IconPhrase"":""Cloudy"",""HasPrecipitation"":false,""IsDaylight"":true,""Temperature"":{""Value"":51.0,""Unit"":""F"",""UnitType"":18},""RealFeelTemperature"":{""Value"":49.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""RealFeelTemperatureShade"":{""Value"":47.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""WetBulbTemperature"":{""Value"":42.0,""Unit"":""F"",""UnitType"":18},""DewPoint"":{""Value"":30.0,""Unit"":""F"",""UnitType"":18},""Wind"":{""Speed"":{""Value"":8.1,""Unit"":""mi/h"",""UnitType"":9},""Direction"":{""Degrees"":301,""Localized"":""WNW"",""English"":""WNW""}},""WindGust"":{""Speed"":{""Value"":10.4,""Unit"":""mi/h"",""UnitType"":9}},""RelativeHumidity"":44,""IndoorRelativeHumidity"":38,""Visibility"":{""Value"":10.0,""Unit"":""mi"",""UnitType"":2},""Ceiling"":{""Value"":7100.0,""Unit"":""ft"",""UnitType"":0},""UVIndex"":1,""UVIndexText"":""Low"",""PrecipitationProbability"":6,""ThunderstormProbability"":0,""RainProbability"":6,""SnowProbability"":0,""IceProbability"":0,""TotalLiquid"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Rain"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Snow"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Ice"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""CloudCover"":91,""Evapotranspiration"":{""Value"":0.01,""Unit"":""in"",""UnitType"":1},""SolarIrradiance"":{""Value"":226.2,""Unit"":""W/m²"",""UnitType"":33},""MobileLink"":"""",""Link"":""""},{""DateTime"":""2023-04-22T18:00:00-06:00"",""EpochDateTime"":1682208000,""WeatherIcon"":7,""IconPhrase"":""Cloudy"",""HasPrecipitation"":false,""IsDaylight"":true,""Temperature"":{""Value"":52.0,""Unit"":""F"",""UnitType"":18},""RealFeelTemperature"":{""Value"":49.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""RealFeelTemperatureShade"":{""Value"":48.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""WetBulbTemperature"":{""Value"":42.0,""Unit"":""F"",""UnitType"":18},""DewPoint"":{""Value"":29.0,""Unit"":""F"",""UnitType"":18},""Wind"":{""Speed"":{""Value"":8.1,""Unit"":""mi/h"",""UnitType"":9},""Direction"":{""Degrees"":339,""Localized"":""NNW"",""English"":""NNW""}},""WindGust"":{""Speed"":{""Value"":9.2,""Unit"":""mi/h"",""UnitType"":9}},""RelativeHumidity"":42,""IndoorRelativeHumidity"":38,""Visibility"":{""Value"":10.0,""Unit"":""mi"",""UnitType"":2},""Ceiling"":{""Value"":7100.0,""Unit"":""ft"",""UnitType"":0},""UVIndex"":1,""UVIndexText"":""Low"",""PrecipitationProbability"":6,""ThunderstormProbability"":0,""RainProbability"":6,""SnowProbability"":0,""IceProbability"":0,""TotalLiquid"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Rain"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Snow"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Ice"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""CloudCover"":90,""Evapotranspiration"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""SolarIrradiance"":{""Value"":161.2,""Unit"":""W/m²"",""UnitType"":33},""MobileLink"":"""",""Link"":""""},{""DateTime"":""2023-04-22T19:00:00-06:00"",""EpochDateTime"":1682211600,""WeatherIcon"":7,""IconPhrase"":""Cloudy"",""HasPrecipitation"":false,""IsDaylight"":true,""Temperature"":{""Value"":51.0,""Unit"":""F"",""UnitType"":18},""RealFeelTemperature"":{""Value"":47.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""RealFeelTemperatureShade"":{""Value"":47.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""WetBulbTemperature"":{""Value"":41.0,""Unit"":""F"",""UnitType"":18},""DewPoint"":{""Value"":29.0,""Unit"":""F"",""UnitType"":18},""Wind"":{""Speed"":{""Value"":8.1,""Unit"":""mi/h"",""UnitType"":9},""Direction"":{""Degrees"":43,""Localized"":""NE"",""English"":""NE""}},""WindGust"":{""Speed"":{""Value"":9.2,""Unit"":""mi/h"",""UnitType"":9}},""RelativeHumidity"":44,""IndoorRelativeHumidity"":38,""Visibility"":{""Value"":10.0,""Unit"":""mi"",""UnitType"":2},""Ceiling"":{""Value"":8000.0,""Unit"":""ft"",""UnitType"":0},""UVIndex"":0,""UVIndexText"":""Low"",""PrecipitationProbability"":4,""ThunderstormProbability"":0,""RainProbability"":4,""SnowProbability"":0,""IceProbability"":0,""TotalLiquid"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Rain"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Snow"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Ice"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""CloudCover"":91,""Evapotranspiration"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""SolarIrradiance"":{""Value"":75.4,""Unit"":""W/m²"",""UnitType"":33},""MobileLink"":"""",""Link"":""""},{""DateTime"":""2023-04-22T20:00:00-06:00"",""EpochDateTime"":1682215200,""WeatherIcon"":2,""IconPhrase"":""Mostly sunny"",""HasPrecipitation"":false,""IsDaylight"":true,""Temperature"":{""Value"":48.0,""Unit"":""F"",""UnitType"":18},""RealFeelTemperature"":{""Value"":45.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""RealFeelTemperatureShade"":{""Value"":45.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""WetBulbTemperature"":{""Value"":40.0,""Unit"":""F"",""UnitType"":18},""DewPoint"":{""Value"":30.0,""Unit"":""F"",""UnitType"":18},""Wind"":{""Speed"":{""Value"":6.9,""Unit"":""mi/h"",""UnitType"":9},""Direction"":{""Degrees"":68,""Localized"":""ENE"",""English"":""ENE""}},""WindGust"":{""Speed"":{""Value"":8.1,""Unit"":""mi/h"",""UnitType"":9}},""RelativeHumidity"":49,""IndoorRelativeHumidity"":39,""Visibility"":{""Value"":10.0,""Unit"":""mi"",""UnitType"":2},""Ceiling"":{""Value"":9000.0,""Unit"":""ft"",""UnitType"":0},""UVIndex"":0,""UVIndexText"":""Low"",""PrecipitationProbability"":0,""ThunderstormProbability"":0,""RainProbability"":0,""SnowProbability"":0,""IceProbability"":0,""TotalLiquid"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Rain"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Snow"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Ice"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""CloudCover"":25,""Evapotranspiration"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""SolarIrradiance"":{""Value"":0.0,""Unit"":""W/m²"",""UnitType"":33},""MobileLink"":"""",""Link"":""""},{""DateTime"":""2023-04-22T21:00:00-06:00"",""EpochDateTime"":1682218800,""WeatherIcon"":34,""IconPhrase"":""Mostly clear"",""HasPrecipitation"":false,""IsDaylight"":false,""Temperature"":{""Value"":46.0,""Unit"":""F"",""UnitType"":18},""RealFeelTemperature"":{""Value"":43.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""RealFeelTemperatureShade"":{""Value"":43.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""WetBulbTemperature"":{""Value"":40.0,""Unit"":""F"",""UnitType"":18},""DewPoint"":{""Value"":31.0,""Unit"":""F"",""UnitType"":18},""Wind"":{""Speed"":{""Value"":6.9,""Unit"":""mi/h"",""UnitType"":9},""Direction"":{""Degrees"":73,""Localized"":""ENE"",""English"":""ENE""}},""WindGust"":{""Speed"":{""Value"":8.1,""Unit"":""mi/h"",""UnitType"":9}},""RelativeHumidity"":55,""IndoorRelativeHumidity"":39,""Visibility"":{""Value"":10.0,""Unit"":""mi"",""UnitType"":2},""Ceiling"":{""Value"":10000.0,""Unit"":""ft"",""UnitType"":0},""UVIndex"":0,""UVIndexText"":""Low"",""PrecipitationProbability"":0,""ThunderstormProbability"":0,""RainProbability"":0,""SnowProbability"":0,""IceProbability"":0,""TotalLiquid"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Rain"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Snow"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Ice"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""CloudCover"":25,""Evapotranspiration"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""SolarIrradiance"":{""Value"":0.0,""Unit"":""W/m²"",""UnitType"":33},""MobileLink"":"""",""Link"":""""}]";
            return ParseHourlyForecast(testJson);
# endif
            HttpResponseMessage response = await _retryPolicy.ExecuteAsync(() => client.GetAsync(path));

            if (response.IsSuccessStatusCode)
            {
                var jsonResult = await response.Content.ReadAsStringAsync();
                _logger.LogInformation(stringBuilder.ToString().TrimEnd('\n'));

                return ParseHourlyForecast(jsonResult);
            }
            else
            {
                stringBuilder.AppendLine($"\t!{MethodBase.GetCurrentMethod()} Unsuccessful...");
                stringBuilder.AppendLine($"\t ResponseCode: {response.StatusCode}");
                stringBuilder.AppendLine($"\t ReasonPhrase: {response.ReasonPhrase}");

                _logger.LogCritical(stringBuilder.ToString().TrimEnd('\n'));

                return null;
            }
        }

        private AccuWeatherForecast[] ParseHourlyForecast(string result)
        {
            _logger.LogTrace("AccuWeather - ParseHourlyForecast");
            return AccuWeatherForecast.FromJson(result);
        }

        internal void ManageForecast()
        {
            StringBuilder stringBuilder = new();
            bool refreshForecast = false;
            var now = DateTime.Now;

            // get forcast if forecast is null or greater than 3 hours old
            if (_forecast == null)
            {
                refreshForecast = true;
                stringBuilder.AppendLine("AccuWeather - Refreshing AccuWeather forecast\n\t no forecast present");
            }
            else if (now >= _forecast.ElementAtOrDefault(3).DateTime)
            {
                refreshForecast = true;
                stringBuilder.AppendLine("AccuWeather - Refreshing AccuWeather forecast\n\t forecast is more than 3 hours old");
            }

            if (refreshForecast)
            {
                _forecast = GetForecast().Result;
                _logger.LogDebug(stringBuilder.ToString().TrimEnd('\n'));
            }
            else
            {
                _logger.LogTrace("AccuWeather - Refresh AccuWeather forecast not necessary");
            }
        }
    }
}
