using ConfigurationSettings;
using Forecast;
using PowerProduction;
using System.Runtime.CompilerServices;
using System.Text;
using TeslaControl;

[assembly: InternalsVisibleTo("PowerConsumptionOptimizer.Tests")]
namespace PowerConsumptionOptimizer
{
    public class ConsumptionOptimizer : IConsumptionOptimizer
    {
        private readonly ILogger<ConsumptionOptimizer> _logger;
        private readonly IConfiguration _configuration;
        private readonly IPowerProduction _powerProduction;
        private readonly IForecast _forecast;
        private readonly ITeslaControl _teslaControl;

        internal List<Vehicle> vehicles;

        private static HelperSettings helperSettings;
        private static COSettings coSettings;

        private static CancellationTokenSource tokenSource;
        private static CancellationToken cancelationToken;

        private static bool exit = false;
        //private static double? solarIrradianceNextHour;

        public ConsumptionOptimizer(ILogger<ConsumptionOptimizer> logger, IConfiguration configuration, IPowerProduction powerProduction, IForecast forecast, ITeslaControl teslaControl)
        {
            _logger = logger;
            _configuration = configuration;
            _powerProduction = powerProduction;
            _forecast = forecast;
            _teslaControl = teslaControl;

            vehicles = new();
            _configuration.GetSection("Vehicles").Bind(vehicles);
            coSettings = new();
            _configuration.GetSection("ConsumptionOptimizer").Bind(coSettings);
            helperSettings = new();
            _configuration.GetSection("Helpers").Bind(helperSettings);
        }

        public async Task Optimize()
        {
            List<Task> tasks;

            while (!exit)
            {
                try
                {
                    tasks = new();
                    tokenSource = new();
                    cancelationToken = tokenSource.Token;

                    tasks.Add(Task.Run(() => RefreshVehicleChargeState(vehicles, 15), tokenSource.Token));
                    Thread.Sleep(5000); //sleep for a short time to improve messaging
                    await Parallel.ForEachAsync(vehicles, async (vehicle, cancelationToken) =>
                    {
                        //delay until vehicle charge state has been updated for the first time
                        while (vehicle.ChargeState == null)
                        {
                            _logger.LogDebug($"{@vehicle.Name} - Waiting for ChargeState refresh");
                            await Task.Delay(TimeSpan.FromSeconds(15));
                        }
                    });

                    if (vehicles.Count > 0)
                    {
                        tasks.Add(Task.Run(() => RefreshVehicleChargePriority(vehicles, 30), tokenSource.Token));
                    }

                    //tasks.Add(Task.Run(() => RefreshNetPowerProduction(vehicles, 1), tokenSource.Token));

                    //solarIrradianceNextHour = null; // having this prevents a race condition where the application goes to sleep before SolarIrradiance is updated
                    //tasks.Add(Task.Run(() => RefreshSolarIrradianceNextHour(120), tokenSource.Token));
                    //delay until solarIrradianceNexHour has been updated for the first time
                    //while (solarIrradianceNextHour == null)
                    //{
                    //    _logger.LogDebug($"Waiting for SolarIrradianceNextHour refresh");
                    //    await Task.Delay(TimeSpan.FromSeconds(5));
                    //}
                    tasks.Add(Task.Run(() => DetermineMonitorCharging(vehicles, 61)));

                    //wait until all the tasks in the list are completed
                    await Task.WhenAll(tasks); //throws an exception when a task is canceled using the cancelation token
                }
                catch (TaskCanceledException ex)
                {
                    // do nothing and continue on

                    //TODO: consider foreach though the exceptions and log which tasks were canceled
                    _logger.LogError($"ExceptionType: {ex.GetType().Name}; ExceptionMessage: {@ex.Message}");
                    _logger.LogDebug($"Continuing after TaskCanceledException...");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"ExceptionType: {ex.GetType().Name}; ExceptionMessage: {@ex.Message}");
                }
                finally
                {
                    tokenSource.Dispose();
                }

                if (!exit)
                {
                    _logger.LogInformation($"Power Consumption Optimizer - Resuming\n\t pause duration has expired");
                }
            }
        }

        /// <summary>
        /// Determine if the monitoring of charging should continue or be paused
        /// </summary>
        /// <param name="sleepDuration">how long to sleep between checks</param>
        /// <returns>task</returns>
        private async Task DetermineMonitorCharging(List<Vehicle> vehicles, int sleepDuration)
        {
            bool monitor = true;
            double solarIrradianceThreshold = Helpers.GetSolarIrradianceThreshold(coSettings, helperSettings);

            while (!exit && monitor)
            {
                StringBuilder output = new();
                output.Append($"Power Consumption Optimizer - ");

                if (_forecast.GetSolarIrradianceNextHour() <= solarIrradianceThreshold)
                {
                    //pause until the hour before reaching the SolarIrradianceThreshold
                    sleepDuration = (Helpers.DetermineSleepDuration(_forecast.GetSolarIrradianceByHour(), coSettings, helperSettings) - 1) * 60;
                    tokenSource.Cancel(); //cancel tasks

                    output.AppendLine($"Pause active monitoring until {DateTime.Now.AddMinutes(sleepDuration)}");
                    output.AppendLine($"\t SolarIrradiance is less than {solarIrradianceThreshold} over the next {sleepDuration / 60} hour(s)");
                    monitor = false;
                }
                else if (GetPriorityVehicle() is null)
                {
                    tokenSource.Cancel(); //cancel tasks
                    sleepDuration *= 2;

                    output.AppendLine($"Pause active monitoring until {DateTime.Now.AddMinutes(sleepDuration)}");
                    output.AppendLine($"\t all vehicle(s) are either at their charge limit or unavailable for charging");
                    monitor = false;
                }
                else
                {
                    output.AppendLine("Continuing active monitoring");
                    output.AppendLine($"\t SolarIrradiance is greater than {solarIrradianceThreshold} over the next hour");
                }
                _logger.LogInformation(output.ToString().TrimEnd('\n'));
                output.Clear();
                await Task.Delay(TimeSpan.FromMinutes(sleepDuration));
            }
        }

        /// <summary>
        /// Refreshed the current projected solar irradiance over the next 12 hours
        /// </summary>
        /// <param name="sleepDuration">milliseconds to sleep between refresh cycles</param>
        //private void RefreshSolarIrradianceNextHour(int sleepDuration)
        //{
        //    while (!cancelationToken.IsCancellationRequested)
        //    {
        //        solarIrradianceNextHour = _forecast.GetSolarIrradianceNextHour();

        //        cancelationToken.WaitHandle.WaitOne(TimeSpan.FromMinutes(sleepDuration));
        //    }
        //}

        /// <summary>
        /// Cycles though a list of vehicles and if needed, refreshes their Charge State
        /// </summary>
        /// <param name="vehicles"></param>
        /// <param name="sleepDuration">second so sleep between loops</param>
        private void RefreshVehicleChargeState(List<Vehicle> vehicles, int sleepDuration)
        {
            while (!cancelationToken.IsCancellationRequested)
            {
                StringBuilder output = new();

                foreach (Vehicle vehicle in vehicles)
                {
                    output.AppendLine($"{vehicle.Name} - GetVehicleChargeState");
                    vehicle.RefreshChargeState = false;
                    vehicle.ChargeState = _teslaControl.GetVehicleChargeStateAsync(vehicle.Id).Result;

                    output.AppendLine($"\t {vehicle.Name} - ChargingState: {vehicle.ChargeState.ChargingState}");
                    output.AppendLine($"\t {vehicle.Name} - BatteryLevel: {vehicle.ChargeState.BatteryLevel}");
                    output.AppendLine($"\t {vehicle.Name} - ChargeLimitStateOfCharge: {vehicle.ChargeState.ChargeLimitStateOfCharge}");
                    output.AppendLine($"\t {vehicle.Name} - ChargeAmps: {vehicle.ChargeState.ChargeAmps}");
                    output.Append($"\t {vehicle.Name} - IsPriority: {vehicle.IsPriority}");

                    _logger.LogInformation(output.ToString());
                    output.Clear();
                }
                cancelationToken.WaitHandle.WaitOne(TimeSpan.FromMinutes(sleepDuration));
            }
        }

        private void RefreshVehicleChargePriority(List<Vehicle> vehicles, int sleepDuration)
        {
            while (!cancelationToken.IsCancellationRequested)
            {
                var resultLog = DetermineChargingPriority();
                _logger.LogInformation(resultLog.ToString());
                cancelationToken.WaitHandle.WaitOne(TimeSpan.FromMinutes(sleepDuration));
            }
        }

        /// <summary>
        /// Refreshes the current net power production value
        /// </summary>
        /// <param name="vehicles">list of type Vehicle</param>
        /// <param name="sleepDuration">sleep duration in minutes</param>
        private void RefreshNetPowerProduction(List<Vehicle> vehicles, int sleepDuration)
        {
            double previousNetPowerProduction = 0.0;
            int desiredAmps;

            while (!cancelationToken.IsCancellationRequested)
            {
                var netPowerProduction = (double)_powerProduction.GetNetPowerProduction();
                var powerProductionChange = previousNetPowerProduction - netPowerProduction;

                _logger.LogInformation($"Net Power Production Change: {Math.Abs((double)powerProductionChange)} watts");

                foreach (Vehicle vehicle in vehicles)
                {
                    desiredAmps = Helpers.CalculateDesiredAmps(helperSettings, vehicle, netPowerProduction);
                    if (ChangeChargeRate(vehicle, desiredAmps))
                    {
                        vehicle.RefreshChargeState = true;
                        previousNetPowerProduction = netPowerProduction; // only update when true otherwise could miss a slow moving change
                        SetChargeRate(vehicle, desiredAmps);
                    }
                }
                cancelationToken.WaitHandle.WaitOne(TimeSpan.FromMinutes(sleepDuration));
            }
        }

        /// <summary>
        /// Set the the vehicle charge rate
        /// </summary>
        /// <param name="vehicle">the vehicle to set the charge rate for</param>
        /// <param name="desiredAmps">the amps to set the charge rate at</param>
        private void SetChargeRate(Vehicle vehicle, int desiredAmps)
        {
            StringBuilder output = new();

            vehicle.ChargeState = _teslaControl.GetVehicleChargeStateAsync(vehicle.Id).Result;

            if (desiredAmps < 5)
            {
                _teslaControl.ChargeStopAsync(vehicle.Id);
                _teslaControl.SetChargingAmpsAsync(vehicle.Id, 5);
                vehicle.ChargeState.ChargingState = "Stopped";
                vehicle.RefreshChargeState = true;
                output.AppendLine($"\t {vehicle.Name} - Desired Amps is {desiredAmps} ... charging stopped");
                output.AppendLine($"\t {vehicle.Name} - SetChargingAmps to default");
            }
            else
            {
                _teslaControl.SetChargingAmpsAsync(vehicle.Id, desiredAmps);
                vehicle.ChargeState.ChargeAmps = desiredAmps;
                if (!vehicle.ChargeState.ChargeEnableRequest)
                {
                    _teslaControl.ChargeStartAsync(vehicle.Id);
                    vehicle.ChargeState.ChargingState = "Charging";
                    vehicle.RefreshChargeState = true;
                    output.AppendLine($"\t {vehicle.Name} - Desired Amps is {desiredAmps} ... charging started");
                }
                output.AppendLine($"\t {vehicle.Name} - SetChargingAmps to: {desiredAmps}");
            }
            _logger.LogInformation(output.ToString().TrimEnd('\n'));
        }

        /// <summary>
        /// Determines if the vehicles current charge state or rate should be changed
        /// </summary>
        /// <param name="vehicle">vehicle to be acted upon</param>
        /// <param name="desiredAmps">the calculated number of amps desired for charging</param>
        /// <returns>true if the vehicle charge needs to be changed, else false</returns>
        internal bool ChangeChargeRate(Vehicle vehicle, int desiredAmps)
        {
            StringBuilder reason = new();
            bool changeCharging = true;

            if (desiredAmps == vehicle.ChargeState.ChargeAmps && vehicle.ChargeState.ChargingState == "Charging")
            {
                changeCharging = false;
                reason.AppendLine("\t already charging at available amps");
            }

            if (vehicle.ChargeState.BatteryLevel >= vehicle.ChargeState.ChargeLimitStateOfCharge)
            {
                changeCharging = false;
                reason.AppendLine($"\t vehicle is at its charge limit of {vehicle.ChargeState.ChargeLimitStateOfCharge}%");
            }

            if (desiredAmps < 5 && vehicle.ChargeState.ChargingState == "Stopped")
            {
                changeCharging = false;
                reason.AppendLine("\t available amps is < 5 and charging is already stopped");
            }

            if (vehicle.ChargeState.ChargingState == "Stopped" && vehicle.ChargeState.ChargeAmps != 5)
            {
                changeCharging = true;
                reason.AppendLine("\t charging is stopped and charge amps is not set to 5");
            }

            if (changeCharging == true)
            {
                reason.AppendLine("\t all required conditions met");
            }

            if (!vehicle.IsPriority && vehicle.ChargeState.ChargingState == "Charging")
            {
                changeCharging = true;
                vehicle.RefreshChargeState = true;
                reason.Clear();
                reason.AppendLine("\t vehicle is charging but is no longer the charging priority");
            }

            vehicle.RefreshChargeState = changeCharging; // if charging is changed then refresh the chargeState
            _logger.LogInformation($"{vehicle.Name} - QueueChangeChargeRate: {changeCharging}\n {reason.ToString().TrimEnd('\n')}");
            return changeCharging;
        }

        /// <summary>
        /// Determines if the vehicle charge state information should be refreshed
        /// </summary>
        /// <param name="netPowerProduction">current net power production</param>
        /// <param name="previousNetPowerProduction">previous net power production</param>
        /// <param name="chargerVoltage">current voltage provided by charger</param>
        /// <returns>true or false</returns>
        internal void RefreshVehicleChargeState(Vehicle vehicle, double? previousNetPowerProduction, double? netPowerProduction)
        {
            StringBuilder reason = new();
            vehicle.RefreshChargeState = false;
            var powerProductionChange = previousNetPowerProduction - netPowerProduction;

            if (vehicle.IsPriority)
            {
                reason.AppendLine("\t vehicle is the charging priority");
            }
            else
            {
                reason.AppendLine("\t vehicle is not the charging priority");
            }

            if (vehicle.ChargeState.ChargingState == "Charging")
            {
                reason.AppendLine("\t vehicle is charging");
            }
            else
            {
                reason.AppendLine("\t vehicle is not charging");
            }

            // vehicle is not the priority but is charging
            if (!vehicle.IsPriority && vehicle.ChargeState.ChargingState == "Charging")
            {
                vehicle.RefreshChargeState = true;
            }
            // significant change in power production
            if (vehicle.IsPriority && vehicle.ChargeState.ChargingState == "Stopped" && Math.Abs((double)powerProductionChange) > 5 * vehicle.ChargeState.ChargerVoltage)
            {
                reason.AppendLine("\t there was a significant change in net power production");
                vehicle.RefreshChargeState = true;
            }
            // vehicle is charging and net power production change is big enough to support a change in the charge rate
            if (vehicle.IsPriority && vehicle.ChargeState.ChargingState == "Charging" && Math.Abs((double)powerProductionChange) > vehicle.ChargeState.ChargerVoltage)
            {
                reason.AppendLine("\t there is enough change in net power production to support an amperage change");
                vehicle.RefreshChargeState = true;
            }
            // vehicle is not charging and net power production can support 5 or more amps
            else if (vehicle.IsPriority && vehicle.ChargeState.ChargingState == "Stopped" && Math.Abs((double)powerProductionChange) > vehicle.ChargeState.ChargerVoltage && Helpers.CalculateDesiredAmps(helperSettings, vehicle, netPowerProduction) >= 5)
            {
                reason.AppendLine("\t there is enough change in net power production to start charging");
                vehicle.RefreshChargeState = true;
            }

            _logger.LogInformation($"{vehicle.Name} - Queue RefreshVehicleChargeState: {vehicle.RefreshChargeState}\n {reason.ToString().TrimEnd('\n')}");
        }

        internal string DetermineChargingPriority()
        {
            Dictionary<string, double> priority = new();
            StringBuilder reason = new();
            reason.AppendLine("Determine Vehicle Charging Priority");

            foreach (Vehicle vehicle in vehicles)
            {
                double priorityScore = 0;

                //deduct 1 point for being disconnected
                if (vehicle.ChargeState.ChargingState == "Disconnected")
                {
                    priorityScore += -10;
                    reason.AppendLine($"\t {vehicle.Name} - was deducted 10 priority points for being disconnected from a charger");
                }

                //deduct 1 point charging being complete
                if (vehicle.ChargeState.ChargingState == "Complete")
                {
                    priorityScore += -1;
                    reason.AppendLine($"\t {vehicle.Name} - was deducted 1 priority point for being at charge limit");
                }

                // give slight preference to vehicle that currently has priority
                if (vehicle.IsPriority)
                {
                    priorityScore += .001;
                    reason.AppendLine($"\t {vehicle.Name} - was given .001 priority points for currently having priority");
                }
                // give priority to vehicles with a Batterylevel < than 50
                if (vehicle.ChargeState.BatteryLevel < 50)
                {
                    priorityScore += 1;
                    reason.AppendLine($"\t {vehicle.Name} - was given 1 priority point for having a battery level below 50%");
                }

                double percentMissing = (vehicle.ChargeState.ChargeLimitStateOfCharge - (double)vehicle.ChargeState.BatteryLevel) / 100;
                reason.AppendLine($"\t {vehicle.Name} - was given {percentMissing} priority points for being {Math.Round(percentMissing * 100, 0)}% below its charge limit of {vehicle.ChargeState.ChargeLimitStateOfCharge}%");
                priorityScore += percentMissing;
                reason.AppendLine($"\t {vehicle.Name} - total priority score is {priorityScore}");

                priority.Add(vehicle.Id, priorityScore);
            }

            if (priority.Where(v => v.Value > 0.011).Count() > 0)
            {
                var priorityVehicleId = priority.OrderByDescending(v => v.Value).First().Key;

                foreach (Vehicle vehicle in vehicles)
                {
                    vehicle.IsPriority = vehicle.Id == priorityVehicleId ? true : false;
                }

                var priorityVehicle = GetPriorityVehicle();
                reason.AppendLine($"\t {priorityVehicle.Name} - was given charging priority");
            }
            else
            {
                foreach (Vehicle vehicle in vehicles)
                {
                    vehicle.IsPriority = false;
                }
                reason.AppendLine($"\t neither vehicle was given charging priority");
            }

            return reason.ToString().TrimEnd('\n');
        }

        public Vehicle GetPriorityVehicle()
        {
            foreach (Vehicle vehicle in vehicles)
            {
                if (vehicle.IsPriority)
                    return vehicle;
            }
            return null;
        }
    }
}

