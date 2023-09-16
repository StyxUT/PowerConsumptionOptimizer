using ConfigurationSettings;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PowerConsumptionOptimizer.Test")]
namespace PowerConsumptionOptimizer
{
    public static class Helpers
    {
        /// <summary>
        /// Use sigmoid function to normalize input value
        /// </summary>
        /// <param name="x">value to be normalized</param>
        /// <param name="expectedMin">expected minimum range value</param>
        /// <param name="expectedMax">expected maximum range value</param>
        /// <param name="invert">invert sigmod function</param>
        /// <returns>normalized double in the range of -1 through +1</returns>
        internal static double Normalize(double x, double expectedMin, double expectedMax, bool invert = false)
        {

            // https://www.desmos.com/calculator/m5ga4dpo3x
            double i = (expectedMin + expectedMax) / 2; // inflection point
            double k = 1 / (expectedMax - expectedMin) * 10; // steepness
            k *= (invert ? -1 : 1); // invert if needed

            return -2 * ((1 / (Math.Exp(k * (x - i)) + 1)) - .5);

        }

        /// <summary>
        /// Determine application sleep duration in hours
        /// </summary>
        /// <param name="irradiances">list of SolarIrradiance values by hour</param>
        /// <returns>sleep duration in hours</returns>
        public static int DetermineSleepDuration(List<double> irradiances, COSettings pcoSettings, HelperSettings helperSettings)
        {
            // increment until the next hour where the SolarIrradiationThreshold is met
            double solarIrradianceThreshold = Helpers.GetSolarIrradianceThreshold(pcoSettings, helperSettings);
            int sleepHours = 0;

            foreach (double irradiance in irradiances)
            {
                if (irradiance <= solarIrradianceThreshold)
                {
                    sleepHours++;
                }
                else
                {
                    break;
                }
            }

            return sleepHours;
        }

        internal static int CalculateDesiredAmps(HelperSettings helperSettings, Vehicle vehicle, double? netPowerProduction)
        {
            int currentAmps;
            int currentVoltage;

            // only the priority vehicle should charge
            if (!vehicle.IsPriority)
                return 0;

            // is not the priority or is not in a chargable state i.e. disconnected, at charging limit, in maintenance mode, etc.
            if (!vehicle.IsPriority || (vehicle.ChargeState.ChargingState != "Charging" && vehicle.ChargeState.ChargingState != "Stopped"))
            {
                return 0;
            }
            else if (vehicle.ChargeState.ChargingState == "Charging")
            {
                currentAmps = vehicle.ChargeState.ChargeAmps;
                currentVoltage = vehicle.ChargeState.ChargerVoltage < 100 ? helperSettings.DefaultChargerVoltage : vehicle.ChargeState.ChargerVoltage;
            }
            // if battery level is less than charge Override Percentage, charge at Charge Override Amps regardless of net power production
            else if (vehicle.ChargeState.ChargingState == "Stopped" && vehicle.ChargeState.BatteryLevel <= helperSettings.ChargeOverridePercenage)
            {
                return helperSettings.ChargeOverrideAmps;
            }
            else // able to charge but is stopped
            {
                currentAmps = 0;
                currentVoltage = helperSettings.DefaultChargerVoltage;
            }

            double wattsAvailable = ((double)netPowerProduction - helperSettings.WattBuffer) + (currentAmps * currentVoltage); //available - buffer + used by charger
            double desiredAmps = wattsAvailable / currentVoltage;

            if (desiredAmps >= 48)
                return 48;
            else if (desiredAmps <= 0)
                return 0;
            else
                return (int)desiredAmps; // truncate at decimal
        }

        /// <summary>
        /// Gets the correct solar irradiance threshold on AM/PM and app setting
        /// </summary>
        /// <param name="pcoSettings">app settings</param>
        /// <returns>current solar irradiance threshold</returns>
        public static double GetSolarIrradianceThreshold(COSettings pcoSettings, HelperSettings helperSettings)
        {
            DateTime dateTime = DateTime.UtcNow.AddHours(helperSettings.UTCOffset);
            return dateTime.ToString("tt", System.Globalization.CultureInfo.InvariantCulture) == "AM" ? pcoSettings.AmSolarIrradianceThreshold : pcoSettings.PmSolarIrradianceThreshold;
        }

        public static bool TimeBetween(DateTime datetime, TimeSpan start, TimeSpan end)
        {
            // convert datetime to a TimeSpan
            TimeSpan now = datetime.TimeOfDay;
            // see if start comes before end
            if (start < end)
                return start <= now && now <= end;
            // start is after end, so do the inverse comparison
            return !(end < now && now < start);
        }
    }
}