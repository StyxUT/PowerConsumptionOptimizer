using PowerConsumptionOptimizer;

namespace ConfigurationSettings
{
    public class HelperSettings
    {
        public const string HS = "HelperSettings";

        public int WattBuffer { get; set; }
        public int DefaultChargerVoltage { get; set; }
        public int ChargeOverridePercenage { get; set; }
        public int ChargeOverrideAmps { get; set; }
        public int UTCOffset { get; set; }
    }

    public class VehicleSettings
    {
        public const string VS = "VehicleSettings";
        public List<Vehicle> Vehicles { get; set; }
    }

    public class ConsumptionOptimizerSettings
    {
        public const string COS = "ConsumptionOptimizerSettings";

        public int IrradianceSleepThreshold { get; set; }
        public string ForecastServiceURL { get; set; }
    }

    public class TeslaSettings
    {
        public const string TS = "TeslaSettings";
        public string TeslaRefreshToken { get; set; }
    }

    public class PowerProductionSettings
    {
        public const string PPS = "PowerProductionSettings";
        public string EnvoyToken { get; set; }
    }
}
