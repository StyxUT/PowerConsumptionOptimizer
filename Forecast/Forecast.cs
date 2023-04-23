namespace Forecast
{
    using System;
    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class AccuWeatherForecast
    {
        [JsonProperty("DateTime")]
        public DateTimeOffset DateTime { get; set; }

        [JsonProperty("EpochDateTime")]
        public long EpochDateTime { get; set; }

        [JsonProperty("WeatherIcon")]
        public long WeatherIcon { get; set; }

        [JsonProperty("IconPhrase")]
        public IconPhrase IconPhrase { get; set; }

        [JsonProperty("HasPrecipitation")]
        public bool HasPrecipitation { get; set; }

        [JsonProperty("IsDaylight")]
        public bool IsDaylight { get; set; }

        [JsonProperty("Temperature")]
        public Ceiling Temperature { get; set; }

        [JsonProperty("RealFeelTemperature")]
        public Ceiling RealFeelTemperature { get; set; }

        [JsonProperty("RealFeelTemperatureShade")]
        public Ceiling RealFeelTemperatureShade { get; set; }

        [JsonProperty("WetBulbTemperature")]
        public Ceiling WetBulbTemperature { get; set; }

        [JsonProperty("DewPoint")]
        public Ceiling DewPoint { get; set; }

        [JsonProperty("Wind")]
        public Wind Wind { get; set; }

        [JsonProperty("WindGust")]
        public WindGust WindGust { get; set; }

        [JsonProperty("RelativeHumidity")]
        public long RelativeHumidity { get; set; }

        [JsonProperty("IndoorRelativeHumidity")]
        public long IndoorRelativeHumidity { get; set; }

        [JsonProperty("Visibility")]
        public Ceiling Visibility { get; set; }

        [JsonProperty("Ceiling")]
        public Ceiling Ceiling { get; set; }

        [JsonProperty("UVIndex")]
        public long UvIndex { get; set; }

        [JsonProperty("UVIndexText")]
        public UvIndexText UvIndexText { get; set; }

        [JsonProperty("PrecipitationProbability")]
        public long PrecipitationProbability { get; set; }

        [JsonProperty("ThunderstormProbability")]
        public long ThunderstormProbability { get; set; }

        [JsonProperty("RainProbability")]
        public long RainProbability { get; set; }

        [JsonProperty("SnowProbability")]
        public long SnowProbability { get; set; }

        [JsonProperty("IceProbability")]
        public long IceProbability { get; set; }

        [JsonProperty("TotalLiquid")]
        public Ceiling TotalLiquid { get; set; }

        [JsonProperty("Rain")]
        public Ceiling Rain { get; set; }

        [JsonProperty("Snow")]
        public Ceiling Snow { get; set; }

        [JsonProperty("Ice")]
        public Ceiling Ice { get; set; }

        [JsonProperty("CloudCover")]
        public long CloudCover { get; set; }

        [JsonProperty("Evapotranspiration")]
        public Ceiling Evapotranspiration { get; set; }

        [JsonProperty("SolarIrradiance")]
        public Ceiling SolarIrradiance { get; set; }

        [JsonProperty("MobileLink")]
        public Uri MobileLink { get; set; }

        [JsonProperty("Link")]
        public Uri Link { get; set; }
    }

    public partial class Ceiling
    {
        [JsonProperty("Value")]
        public double Value { get; set; }

        [JsonProperty("Unit")]
        public Unit Unit { get; set; }

        [JsonProperty("UnitType")]
        public long UnitType { get; set; }

        //[JsonProperty("Phrase", NullValueHandling = NullValueHandling.Ignore)]
        //public Phrase? Phrase { get; set; }
    }

    public partial class Wind
    {
        [JsonProperty("Speed")]
        public Ceiling Speed { get; set; }

        [JsonProperty("Direction")]
        public Direction Direction { get; set; }
    }

    public partial class Direction
    {
        [JsonProperty("Degrees")]
        public long Degrees { get; set; }

        [JsonProperty("Localized")]
        public string Localized { get; set; }

        [JsonProperty("English")]
        public string English { get; set; }
    }

    public partial class WindGust
    {
        [JsonProperty("Speed")]
        public Ceiling Speed { get; set; }
    }

    public enum Phrase { Chilly };

    public enum Unit { F, Ft, In, Mi, MiH, WM };

    public enum IconPhrase { Cloudy, MostlyClear, MostlySunny };

    public enum UvIndexText { Low, Moderate };

    public partial class AccuWeatherForecast
    {
        public static AccuWeatherForecast[] FromJson(string json) => JsonConvert.DeserializeObject<AccuWeatherForecast[]>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this AccuWeatherForecast[] self) => JsonConvert.SerializeObject(self, global::Forecast.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                PhraseConverter.Singleton,
                UnitConverter.Singleton,
                IconPhraseConverter.Singleton,
                UvIndexTextConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class PhraseConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Phrase) || t == typeof(Phrase?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            //if (value == "Chilly")
            //{
            //    return Phrase.Chilly;
            //}
            //throw new Exception("Cannot unmarshal type Phrase");

            return "Not Implemented";
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Phrase)untypedValue;
            //if (value == Phrase.Chilly)
            //{
            //    serializer.Serialize(writer, "Chilly");
            //    return;
            //}
            //throw new Exception("Cannot marshal type Phrase");
            return;
        }

        public static readonly PhraseConverter Singleton = new PhraseConverter();
    }

    internal class UnitConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Unit) || t == typeof(Unit?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "F":
                    return Unit.F;
                case "W/m²":
                    return Unit.WM;
                case "ft":
                    return Unit.Ft;
                case "in":
                    return Unit.In;
                case "mi":
                    return Unit.Mi;
                case "mi/h":
                    return Unit.MiH;
            }
            throw new Exception("Cannot unmarshal type Unit");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Unit)untypedValue;
            switch (value)
            {
                case Unit.F:
                    serializer.Serialize(writer, "F");
                    return;
                case Unit.WM:
                    serializer.Serialize(writer, "W/m²");
                    return;
                case Unit.Ft:
                    serializer.Serialize(writer, "ft");
                    return;
                case Unit.In:
                    serializer.Serialize(writer, "in");
                    return;
                case Unit.Mi:
                    serializer.Serialize(writer, "mi");
                    return;
                case Unit.MiH:
                    serializer.Serialize(writer, "mi/h");
                    return;
            }
            throw new Exception("Cannot marshal type Unit");
        }

        public static readonly UnitConverter Singleton = new UnitConverter();
    }

    internal class IconPhraseConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(IconPhrase) || t == typeof(IconPhrase?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "Cloudy":
                    return IconPhrase.Cloudy;
                case "Mostly clear":
                    return IconPhrase.MostlyClear;
                case "Mostly sunny":
                    return IconPhrase.MostlySunny;
            }
            return "Not Implemented";
            //throw new Exception("Cannot unmarshal type IconPhrase");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            //var value = (IconPhrase)untypedValue;
            //switch (value)
            //{
            //    case IconPhrase.Cloudy:
            //        serializer.Serialize(writer, "Cloudy");
            //        return;
            //    case IconPhrase.MostlyClear:
            //        serializer.Serialize(writer, "Mostly clear");
            //        return;
            //    case IconPhrase.MostlySunny:
            //        serializer.Serialize(writer, "Mostly sunny");
            //        return;
            //}
            //throw new Exception("Cannot marshal type IconPhrase");
            return;
        }

        public static readonly IconPhraseConverter Singleton = new IconPhraseConverter();
    }

    internal class UvIndexTextConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(UvIndexText) || t == typeof(UvIndexText?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "Low":
                    return UvIndexText.Low;
                case "Moderate":
                    return UvIndexText.Moderate;
            }
            return "Not Implemented";
             //throw new Exception("Cannot unmarshal type UvIndexText");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (UvIndexText)untypedValue;
            switch (value)
            {
                case UvIndexText.Low:
                    serializer.Serialize(writer, "Low");
                    return;
                case UvIndexText.Moderate:
                    serializer.Serialize(writer, "Moderate");
                    return;
            }
            return;
            //throw new Exception("Cannot marshal type UvIndexText");
        }

        public static readonly UvIndexTextConverter Singleton = new UvIndexTextConverter();
    }
}
