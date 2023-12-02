using System.Text.Json.Serialization;

namespace PowerConsumptionOptimizer.Forecast
{
    public class SolarData
    {
        [JsonPropertyName("DateTime")]
        public DateTimeOffset DateTime { get; set; }

        [JsonPropertyName("SolarIrradiance")]
        public SolarIrradiance SolarIrradiance { get; set; }
    }

    public class SolarIrradiance
    {
        [JsonPropertyName("Value")]
        public double Value { get; set; }

        [JsonPropertyName("Unit")]
        public string Unit { get; set; }

        [JsonPropertyName("UnitType")]
        public int UnitType { get; set; }
    }

}
