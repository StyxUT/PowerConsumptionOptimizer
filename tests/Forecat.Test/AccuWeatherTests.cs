using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Forecast.Tests
{
    public class AccuWeatherTests
    {
        // Below testJson is no in AccuWeather.cs and used #if DEBUG
        //const string testJson = @"[{""DateTime"":""2023-04-22T10:00:00-06:00"",""EpochDateTime"":1682179200,""WeatherIcon"":7,""IconPhrase"":""Cloudy"",""HasPrecipitation"":false,""IsDaylight"":true,""Temperature"":{""Value"":45.0,""Unit"":""F"",""UnitType"":18},""RealFeelTemperature"":{""Value"":42.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""RealFeelTemperatureShade"":{""Value"":40.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""WetBulbTemperature"":{""Value"":38.0,""Unit"":""F"",""UnitType"":18},""DewPoint"":{""Value"":28.0,""Unit"":""F"",""UnitType"":18},""Wind"":{""Speed"":{""Value"":9.2,""Unit"":""mi/h"",""UnitType"":9},""Direction"":{""Degrees"":310,""Localized"":""NW"",""English"":""NW""}},""WindGust"":{""Speed"":{""Value"":10.4,""Unit"":""mi/h"",""UnitType"":9}},""RelativeHumidity"":52,""IndoorRelativeHumidity"":37,""Visibility"":{""Value"":10.0,""Unit"":""mi"",""UnitType"":2},""Ceiling"":{""Value"":6600.0,""Unit"":""ft"",""UnitType"":0},""UVIndex"":1,""UVIndexText"":""Low"",""PrecipitationProbability"":7,""ThunderstormProbability"":0,""RainProbability"":7,""SnowProbability"":0,""IceProbability"":0,""TotalLiquid"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Rain"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Snow"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Ice"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""CloudCover"":96,""Evapotranspiration"":{""Value"":0.01,""Unit"":""in"",""UnitType"":1},""SolarIrradiance"":{""Value"":254.6,""Unit"":""W/m�"",""UnitType"":33},""MobileLink"":"""",""Link"":""""},{""DateTime"":""2023-04-22T11:00:00-06:00"",""EpochDateTime"":1682182800,""WeatherIcon"":7,""IconPhrase"":""Cloudy"",""HasPrecipitation"":false,""IsDaylight"":true,""Temperature"":{""Value"":46.0,""Unit"":""F"",""UnitType"":18},""RealFeelTemperature"":{""Value"":43.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""RealFeelTemperatureShade"":{""Value"":40.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""WetBulbTemperature"":{""Value"":39.0,""Unit"":""F"",""UnitType"":18},""DewPoint"":{""Value"":29.0,""Unit"":""F"",""UnitType"":18},""Wind"":{""Speed"":{""Value"":9.2,""Unit"":""mi/h"",""UnitType"":9},""Direction"":{""Degrees"":311,""Localized"":""NW"",""English"":""NW""}},""WindGust"":{""Speed"":{""Value"":11.5,""Unit"":""mi/h"",""UnitType"":9}},""RelativeHumidity"":50,""IndoorRelativeHumidity"":38,""Visibility"":{""Value"":10.0,""Unit"":""mi"",""UnitType"":2},""Ceiling"":{""Value"":7100.0,""Unit"":""ft"",""UnitType"":0},""UVIndex"":2,""UVIndexText"":""Low"",""PrecipitationProbability"":7,""ThunderstormProbability"":0,""RainProbability"":7,""SnowProbability"":0,""IceProbability"":0,""TotalLiquid"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Rain"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Snow"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Ice"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""CloudCover"":93,""Evapotranspiration"":{""Value"":0.01,""Unit"":""in"",""UnitType"":1},""SolarIrradiance"":{""Value"":306.2,""Unit"":""W/m�"",""UnitType"":33},""MobileLink"":"""",""Link"":""""},{""DateTime"":""2023-04-22T12:00:00-06:00"",""EpochDateTime"":1682186400,""WeatherIcon"":7,""IconPhrase"":""Cloudy"",""HasPrecipitation"":false,""IsDaylight"":true,""Temperature"":{""Value"":47.0,""Unit"":""F"",""UnitType"":18},""RealFeelTemperature"":{""Value"":46.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""RealFeelTemperatureShade"":{""Value"":41.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""WetBulbTemperature"":{""Value"":40.0,""Unit"":""F"",""UnitType"":18},""DewPoint"":{""Value"":30.0,""Unit"":""F"",""UnitType"":18},""Wind"":{""Speed"":{""Value"":9.2,""Unit"":""mi/h"",""UnitType"":9},""Direction"":{""Degrees"":311,""Localized"":""NW"",""English"":""NW""}},""WindGust"":{""Speed"":{""Value"":11.5,""Unit"":""mi/h"",""UnitType"":9}},""RelativeHumidity"":51,""IndoorRelativeHumidity"":39,""Visibility"":{""Value"":10.0,""Unit"":""mi"",""UnitType"":2},""Ceiling"":{""Value"":7100.0,""Unit"":""ft"",""UnitType"":0},""UVIndex"":3,""UVIndexText"":""Moderate"",""PrecipitationProbability"":7,""ThunderstormProbability"":0,""RainProbability"":7,""SnowProbability"":0,""IceProbability"":0,""TotalLiquid"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Rain"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Snow"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Ice"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""CloudCover"":91,""Evapotranspiration"":{""Value"":0.01,""Unit"":""in"",""UnitType"":1},""SolarIrradiance"":{""Value"":340.2,""Unit"":""W/m�"",""UnitType"":33},""MobileLink"":"""",""Link"":""""},{""DateTime"":""2023-04-22T13:00:00-06:00"",""EpochDateTime"":1682190000,""WeatherIcon"":7,""IconPhrase"":""Cloudy"",""HasPrecipitation"":false,""IsDaylight"":true,""Temperature"":{""Value"":48.0,""Unit"":""F"",""UnitType"":18},""RealFeelTemperature"":{""Value"":47.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""RealFeelTemperatureShade"":{""Value"":43.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""WetBulbTemperature"":{""Value"":40.0,""Unit"":""F"",""UnitType"":18},""DewPoint"":{""Value"":30.0,""Unit"":""F"",""UnitType"":18},""Wind"":{""Speed"":{""Value"":9.2,""Unit"":""mi/h"",""UnitType"":9},""Direction"":{""Degrees"":312,""Localized"":""NW"",""English"":""NW""}},""WindGust"":{""Speed"":{""Value"":11.5,""Unit"":""mi/h"",""UnitType"":9}},""RelativeHumidity"":49,""IndoorRelativeHumidity"":39,""Visibility"":{""Value"":10.0,""Unit"":""mi"",""UnitType"":2},""Ceiling"":{""Value"":7100.0,""Unit"":""ft"",""UnitType"":0},""UVIndex"":3,""UVIndexText"":""Moderate"",""PrecipitationProbability"":7,""ThunderstormProbability"":0,""RainProbability"":7,""SnowProbability"":0,""IceProbability"":0,""TotalLiquid"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Rain"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Snow"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Ice"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""CloudCover"":93,""Evapotranspiration"":{""Value"":0.01,""Unit"":""in"",""UnitType"":1},""SolarIrradiance"":{""Value"":334.6,""Unit"":""W/m�"",""UnitType"":33},""MobileLink"":"""",""Link"":""""},{""DateTime"":""2023-04-22T14:00:00-06:00"",""EpochDateTime"":1682193600,""WeatherIcon"":7,""IconPhrase"":""Cloudy"",""HasPrecipitation"":false,""IsDaylight"":true,""Temperature"":{""Value"":49.0,""Unit"":""F"",""UnitType"":18},""RealFeelTemperature"":{""Value"":47.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""RealFeelTemperatureShade"":{""Value"":44.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""WetBulbTemperature"":{""Value"":41.0,""Unit"":""F"",""UnitType"":18},""DewPoint"":{""Value"":30.0,""Unit"":""F"",""UnitType"":18},""Wind"":{""Speed"":{""Value"":9.2,""Unit"":""mi/h"",""UnitType"":9},""Direction"":{""Degrees"":309,""Localized"":""NW"",""English"":""NW""}},""WindGust"":{""Speed"":{""Value"":11.5,""Unit"":""mi/h"",""UnitType"":9}},""RelativeHumidity"":48,""IndoorRelativeHumidity"":39,""Visibility"":{""Value"":10.0,""Unit"":""mi"",""UnitType"":2},""Ceiling"":{""Value"":7100.0,""Unit"":""ft"",""UnitType"":0},""UVIndex"":2,""UVIndexText"":""Low"",""PrecipitationProbability"":6,""ThunderstormProbability"":0,""RainProbability"":6,""SnowProbability"":0,""IceProbability"":0,""TotalLiquid"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Rain"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Snow"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Ice"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""CloudCover"":95,""Evapotranspiration"":{""Value"":0.01,""Unit"":""in"",""UnitType"":1},""SolarIrradiance"":{""Value"":314.5,""Unit"":""W/m�"",""UnitType"":33},""MobileLink"":"""",""Link"":""""},{""DateTime"":""2023-04-22T15:00:00-06:00"",""EpochDateTime"":1682197200,""WeatherIcon"":7,""IconPhrase"":""Cloudy"",""HasPrecipitation"":false,""IsDaylight"":true,""Temperature"":{""Value"":50.0,""Unit"":""F"",""UnitType"":18},""RealFeelTemperature"":{""Value"":48.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""RealFeelTemperatureShade"":{""Value"":45.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""WetBulbTemperature"":{""Value"":41.0,""Unit"":""F"",""UnitType"":18},""DewPoint"":{""Value"":30.0,""Unit"":""F"",""UnitType"":18},""Wind"":{""Speed"":{""Value"":9.2,""Unit"":""mi/h"",""UnitType"":9},""Direction"":{""Degrees"":289,""Localized"":""WNW"",""English"":""WNW""}},""WindGust"":{""Speed"":{""Value"":10.4,""Unit"":""mi/h"",""UnitType"":9}},""RelativeHumidity"":47,""IndoorRelativeHumidity"":39,""Visibility"":{""Value"":10.0,""Unit"":""mi"",""UnitType"":2},""Ceiling"":{""Value"":7100.0,""Unit"":""ft"",""UnitType"":0},""UVIndex"":2,""UVIndexText"":""Low"",""PrecipitationProbability"":6,""ThunderstormProbability"":0,""RainProbability"":6,""SnowProbability"":0,""IceProbability"":0,""TotalLiquid"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Rain"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Snow"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Ice"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""CloudCover"":96,""Evapotranspiration"":{""Value"":0.01,""Unit"":""in"",""UnitType"":1},""SolarIrradiance"":{""Value"":287.5,""Unit"":""W/m�"",""UnitType"":33},""MobileLink"":"""",""Link"":""""},{""DateTime"":""2023-04-22T16:00:00-06:00"",""EpochDateTime"":1682200800,""WeatherIcon"":7,""IconPhrase"":""Cloudy"",""HasPrecipitation"":false,""IsDaylight"":true,""Temperature"":{""Value"":51.0,""Unit"":""F"",""UnitType"":18},""RealFeelTemperature"":{""Value"":48.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""RealFeelTemperatureShade"":{""Value"":46.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""WetBulbTemperature"":{""Value"":42.0,""Unit"":""F"",""UnitType"":18},""DewPoint"":{""Value"":30.0,""Unit"":""F"",""UnitType"":18},""Wind"":{""Speed"":{""Value"":8.1,""Unit"":""mi/h"",""UnitType"":9},""Direction"":{""Degrees"":285,""Localized"":""WNW"",""English"":""WNW""}},""WindGust"":{""Speed"":{""Value"":10.4,""Unit"":""mi/h"",""UnitType"":9}},""RelativeHumidity"":45,""IndoorRelativeHumidity"":39,""Visibility"":{""Value"":10.0,""Unit"":""mi"",""UnitType"":2},""Ceiling"":{""Value"":7100.0,""Unit"":""ft"",""UnitType"":0},""UVIndex"":1,""UVIndexText"":""Low"",""PrecipitationProbability"":6,""ThunderstormProbability"":0,""RainProbability"":6,""SnowProbability"":0,""IceProbability"":0,""TotalLiquid"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Rain"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Snow"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Ice"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""CloudCover"":94,""Evapotranspiration"":{""Value"":0.01,""Unit"":""in"",""UnitType"":1},""SolarIrradiance"":{""Value"":263.5,""Unit"":""W/m�"",""UnitType"":33},""MobileLink"":"""",""Link"":""""},{""DateTime"":""2023-04-22T17:00:00-06:00"",""EpochDateTime"":1682204400,""WeatherIcon"":7,""IconPhrase"":""Cloudy"",""HasPrecipitation"":false,""IsDaylight"":true,""Temperature"":{""Value"":51.0,""Unit"":""F"",""UnitType"":18},""RealFeelTemperature"":{""Value"":49.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""RealFeelTemperatureShade"":{""Value"":47.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""WetBulbTemperature"":{""Value"":42.0,""Unit"":""F"",""UnitType"":18},""DewPoint"":{""Value"":30.0,""Unit"":""F"",""UnitType"":18},""Wind"":{""Speed"":{""Value"":8.1,""Unit"":""mi/h"",""UnitType"":9},""Direction"":{""Degrees"":301,""Localized"":""WNW"",""English"":""WNW""}},""WindGust"":{""Speed"":{""Value"":10.4,""Unit"":""mi/h"",""UnitType"":9}},""RelativeHumidity"":44,""IndoorRelativeHumidity"":38,""Visibility"":{""Value"":10.0,""Unit"":""mi"",""UnitType"":2},""Ceiling"":{""Value"":7100.0,""Unit"":""ft"",""UnitType"":0},""UVIndex"":1,""UVIndexText"":""Low"",""PrecipitationProbability"":6,""ThunderstormProbability"":0,""RainProbability"":6,""SnowProbability"":0,""IceProbability"":0,""TotalLiquid"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Rain"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Snow"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Ice"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""CloudCover"":91,""Evapotranspiration"":{""Value"":0.01,""Unit"":""in"",""UnitType"":1},""SolarIrradiance"":{""Value"":226.2,""Unit"":""W/m�"",""UnitType"":33},""MobileLink"":"""",""Link"":""""},{""DateTime"":""2023-04-22T18:00:00-06:00"",""EpochDateTime"":1682208000,""WeatherIcon"":7,""IconPhrase"":""Cloudy"",""HasPrecipitation"":false,""IsDaylight"":true,""Temperature"":{""Value"":52.0,""Unit"":""F"",""UnitType"":18},""RealFeelTemperature"":{""Value"":49.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""RealFeelTemperatureShade"":{""Value"":48.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""WetBulbTemperature"":{""Value"":42.0,""Unit"":""F"",""UnitType"":18},""DewPoint"":{""Value"":29.0,""Unit"":""F"",""UnitType"":18},""Wind"":{""Speed"":{""Value"":8.1,""Unit"":""mi/h"",""UnitType"":9},""Direction"":{""Degrees"":339,""Localized"":""NNW"",""English"":""NNW""}},""WindGust"":{""Speed"":{""Value"":9.2,""Unit"":""mi/h"",""UnitType"":9}},""RelativeHumidity"":42,""IndoorRelativeHumidity"":38,""Visibility"":{""Value"":10.0,""Unit"":""mi"",""UnitType"":2},""Ceiling"":{""Value"":7100.0,""Unit"":""ft"",""UnitType"":0},""UVIndex"":1,""UVIndexText"":""Low"",""PrecipitationProbability"":6,""ThunderstormProbability"":0,""RainProbability"":6,""SnowProbability"":0,""IceProbability"":0,""TotalLiquid"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Rain"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Snow"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Ice"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""CloudCover"":90,""Evapotranspiration"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""SolarIrradiance"":{""Value"":161.2,""Unit"":""W/m�"",""UnitType"":33},""MobileLink"":"""",""Link"":""""},{""DateTime"":""2023-04-22T19:00:00-06:00"",""EpochDateTime"":1682211600,""WeatherIcon"":7,""IconPhrase"":""Cloudy"",""HasPrecipitation"":false,""IsDaylight"":true,""Temperature"":{""Value"":51.0,""Unit"":""F"",""UnitType"":18},""RealFeelTemperature"":{""Value"":47.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""RealFeelTemperatureShade"":{""Value"":47.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""WetBulbTemperature"":{""Value"":41.0,""Unit"":""F"",""UnitType"":18},""DewPoint"":{""Value"":29.0,""Unit"":""F"",""UnitType"":18},""Wind"":{""Speed"":{""Value"":8.1,""Unit"":""mi/h"",""UnitType"":9},""Direction"":{""Degrees"":43,""Localized"":""NE"",""English"":""NE""}},""WindGust"":{""Speed"":{""Value"":9.2,""Unit"":""mi/h"",""UnitType"":9}},""RelativeHumidity"":44,""IndoorRelativeHumidity"":38,""Visibility"":{""Value"":10.0,""Unit"":""mi"",""UnitType"":2},""Ceiling"":{""Value"":8000.0,""Unit"":""ft"",""UnitType"":0},""UVIndex"":0,""UVIndexText"":""Low"",""PrecipitationProbability"":4,""ThunderstormProbability"":0,""RainProbability"":4,""SnowProbability"":0,""IceProbability"":0,""TotalLiquid"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Rain"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Snow"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Ice"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""CloudCover"":91,""Evapotranspiration"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""SolarIrradiance"":{""Value"":75.4,""Unit"":""W/m�"",""UnitType"":33},""MobileLink"":"""",""Link"":""""},{""DateTime"":""2023-04-22T20:00:00-06:00"",""EpochDateTime"":1682215200,""WeatherIcon"":2,""IconPhrase"":""Mostly sunny"",""HasPrecipitation"":false,""IsDaylight"":true,""Temperature"":{""Value"":48.0,""Unit"":""F"",""UnitType"":18},""RealFeelTemperature"":{""Value"":45.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""RealFeelTemperatureShade"":{""Value"":45.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""WetBulbTemperature"":{""Value"":40.0,""Unit"":""F"",""UnitType"":18},""DewPoint"":{""Value"":30.0,""Unit"":""F"",""UnitType"":18},""Wind"":{""Speed"":{""Value"":6.9,""Unit"":""mi/h"",""UnitType"":9},""Direction"":{""Degrees"":68,""Localized"":""ENE"",""English"":""ENE""}},""WindGust"":{""Speed"":{""Value"":8.1,""Unit"":""mi/h"",""UnitType"":9}},""RelativeHumidity"":49,""IndoorRelativeHumidity"":39,""Visibility"":{""Value"":10.0,""Unit"":""mi"",""UnitType"":2},""Ceiling"":{""Value"":9000.0,""Unit"":""ft"",""UnitType"":0},""UVIndex"":0,""UVIndexText"":""Low"",""PrecipitationProbability"":0,""ThunderstormProbability"":0,""RainProbability"":0,""SnowProbability"":0,""IceProbability"":0,""TotalLiquid"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Rain"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Snow"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Ice"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""CloudCover"":25,""Evapotranspiration"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""SolarIrradiance"":{""Value"":0.0,""Unit"":""W/m�"",""UnitType"":33},""MobileLink"":"""",""Link"":""""},{""DateTime"":""2023-04-22T21:00:00-06:00"",""EpochDateTime"":1682218800,""WeatherIcon"":34,""IconPhrase"":""Mostly clear"",""HasPrecipitation"":false,""IsDaylight"":false,""Temperature"":{""Value"":46.0,""Unit"":""F"",""UnitType"":18},""RealFeelTemperature"":{""Value"":43.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""RealFeelTemperatureShade"":{""Value"":43.0,""Unit"":""F"",""UnitType"":18,""Phrase"":""Chilly""},""WetBulbTemperature"":{""Value"":40.0,""Unit"":""F"",""UnitType"":18},""DewPoint"":{""Value"":31.0,""Unit"":""F"",""UnitType"":18},""Wind"":{""Speed"":{""Value"":6.9,""Unit"":""mi/h"",""UnitType"":9},""Direction"":{""Degrees"":73,""Localized"":""ENE"",""English"":""ENE""}},""WindGust"":{""Speed"":{""Value"":8.1,""Unit"":""mi/h"",""UnitType"":9}},""RelativeHumidity"":55,""IndoorRelativeHumidity"":39,""Visibility"":{""Value"":10.0,""Unit"":""mi"",""UnitType"":2},""Ceiling"":{""Value"":10000.0,""Unit"":""ft"",""UnitType"":0},""UVIndex"":0,""UVIndexText"":""Low"",""PrecipitationProbability"":0,""ThunderstormProbability"":0,""RainProbability"":0,""SnowProbability"":0,""IceProbability"":0,""TotalLiquid"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Rain"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Snow"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""Ice"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""CloudCover"":25,""Evapotranspiration"":{""Value"":0.0,""Unit"":""in"",""UnitType"":1},""SolarIrradiance"":{""Value"":0.0,""Unit"":""W/m�"",""UnitType"":33},""MobileLink"":"""",""Link"":""""}]";
        private AccuWeather accuWeather = new AccuWeather(NullLogger<AccuWeather>.Instance);

        [Fact]
        public void AccuWeather_SolarIrradianceNextHour_SkipsToCurrentHour()
        {
            //arrange
            DateTime dateTime = DateTime.Now;
            accuWeather.ManageForecast();
            //the first and second forecasts will be skipped because they are in the past
            accuWeather._forecast[2].DateTime = dateTime.AddHours(3);
            //the fourth forecast needs to be set to a time in the future so the forecast isn't refreshed
            accuWeather._forecast[3].DateTime = dateTime.AddHours(4);

            //act
            var result = accuWeather.GetSolarIrradianceNextHour();

            //assert
            Assert.Equal(340.2, result);
        }

        [Fact]
        public void AccuWeather_SolarIrradianceNextHour_ReturnsAllFutureSolarIrradiances()
        {
            //arrange
            DateTime dateTime = DateTime.Now;
            accuWeather.ManageForecast();
            //the first two forecasts will be skipped because they are in the past
            accuWeather._forecast[2].DateTime = dateTime.AddHours(3);
            //the fourth forecast needs to be set to a time in the future so ManageForecast() doesn't refresh the forecast again
            accuWeather._forecast[3].DateTime = dateTime.AddHours(4);
            accuWeather._forecast[4].DateTime = dateTime.AddHours(5);
            accuWeather._forecast[5].DateTime = dateTime.AddHours(6);

            //act
            var result = accuWeather.GetSolarIrradianceByHour();

            //assert
            Assert.Equal(340.2, result.First<double>()); //test first result
            Assert.Equal(0, result.Last<double>()); //test last result
        }
    }
}