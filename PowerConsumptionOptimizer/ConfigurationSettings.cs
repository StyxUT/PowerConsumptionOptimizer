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
        //public const string Vehicles = "Vehicles";
        public double AmSolarIrradianceThreshold { get; set; }
        public double PmSolarIrradianceThreshold { get; set; }

    }

    public class TeslaSettings
    {
        public const string TS = "TeslaSettings";
        public string TeslaRefreshToken { get; set; }
    }
}
