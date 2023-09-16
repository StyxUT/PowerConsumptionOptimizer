//using Forecast;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PowerProduction;
using System;
using System.Collections.Generic;
using TeslaControl;
using Xunit;

namespace PowerConsumptionOptimizer.Tests
{
    public class ConsumptionOptimizerTests
    {
        private readonly Mock<ILogger<ConsumptionOptimizer>> _loggerMock;
        private readonly ConsumptionOptimizer _consumptionOptimizer;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IPowerProduction> _powerProductionMock;
        //private readonly Mock<IForecast> _forecastMock;
        private readonly Mock<ITeslaControl> _teslaControlMock;

        public ConsumptionOptimizerTests()
        {
            _loggerMock = new Mock<ILogger<ConsumptionOptimizer>>();
            _configurationMock = new Mock<IConfiguration>();
            _powerProductionMock = new Mock<IPowerProduction>();
            //_forecastMock = new Mock<IForecast>();
            _teslaControlMock = new Mock<ITeslaControl>();

            var builder = new ConfigurationBuilder();
            builder.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true);
            var configuration = builder.Build();
            _consumptionOptimizer = new ConsumptionOptimizer(_loggerMock.Object, configuration, _powerProductionMock.Object, _teslaControlMock.Object);
        }


        [Theory]
        //desired amps; current charging amps; charge limit; batteryLevel; charging enalbed
        [InlineData(10, 0, 75, 75, "Charging")] // battery reached desired level
        [InlineData(10, 10, 75, 0, "Charging")] // already at desired amps
        [InlineData(4, 5, 99, 75, "Stopped")] // desired amps is < 5 and charging is stopped
        public void PowerConsumptionOptimizer_ChangeChargeRate_IsFalse(int desiredAmps, int chargeAmps, int chargeLimitStateOfCharge, int batteryLevel, string chargingState)
        {
            //arrange
            var chargeState = new TeslaAPI.Models.Vehicles.ChargeState();
            chargeState.ChargeAmps = chargeAmps; // current charging amps
            chargeState.ChargeLimitStateOfCharge = chargeLimitStateOfCharge; // charge limit
            chargeState.BatteryLevel = batteryLevel; // current charge level
            chargeState.ChargingState = chargingState; // charging enabled

            var vehicle = new Vehicle { Name = "test vehicle", Id = "123456", ChargeState = chargeState, IsPriority = true };

            //act
            var result = _consumptionOptimizer.ChangeChargeRate(vehicle, desiredAmps);

            //assert
            Assert.False(result);
        }

        [Theory]
        //IsPriority charging vehicle is true for all tests
        //desired amps; current charging amps; charge limit; batteryLevel; charging enabled
        [InlineData(10, 5, 75, 0, "Charging", true)] // current charging rate is less than desired
        [InlineData(10, 15, 75, 74, "Charging", true)] // current charging rate is greater than desired
        [InlineData(5, 45, 99, 99, "Stopped", true)] // desired amps is >= 5 and charging is not enabled
        [InlineData(5, 5, 75, 74, "Stopped", true)] // desired amps is 5 and charging is not enabled   
        [InlineData(5, 5, 75, 74, "Charging", false)] // vehicle is not priority but is charging   
        public void PowerConsumptionOptimizer_ChangeChargeRate_IsTrue(int desiredAmps, int chargeAmps, int chargeLimitStateOfCharge, int batteryLevel, string chargingState, bool isPriority)
        {
            //arrange
            var chargeState = new TeslaAPI.Models.Vehicles.ChargeState();
            chargeState.ChargeAmps = chargeAmps; // current charging amps
            chargeState.ChargeLimitStateOfCharge = chargeLimitStateOfCharge; // charge limit
            chargeState.BatteryLevel = batteryLevel; // current charge level
            chargeState.ChargingState = chargingState; // charging enabled

            var vehicle = new Vehicle { Name = "test vehicle", Id = "123456", ChargeState = chargeState, IsPriority = isPriority };

            //act
            var result = _consumptionOptimizer.ChangeChargeRate(vehicle, desiredAmps);

            //assert
            Assert.True(result);
        }

        [Fact]
        //vehicle is not the priority but is charging
        public void PowerConsumptionOptimizer_RefreshVehicleChargeState_IsTrue2()
        {
            //arrange
            var chargeState = new TeslaAPI.Models.Vehicles.ChargeState();
            chargeState.ChargingState = "Charging";
                
            var vehicle = new Vehicle { Name = "test vehicle", Id = "123456", ChargeState = chargeState, IsPriority = false };

            //act
            _consumptionOptimizer.RefreshVehicleChargeState(vehicle, 10000, 10000);

            //assert
            Assert.True(vehicle.RefreshChargeState);
        }

        [Theory]
        //IsPriority charging vehicle is true for all tests
        [InlineData(1000, -500, "Charging")] // negative diff larger than voltage; charging
        [InlineData(1000, 2000, "Charging")] // positive diff larger than voltage; charging
        [InlineData(2400, 1800, "Stopped")] // negative diff larger than voltage and can support 5 amps /w buffer; stopped
        [InlineData(-2000, 1800, "Stopped")] // positive diff larger than voltage and can support 5 amps /w buffer; stopped
        [InlineData(-2000, 1000, "Stopped")] // diff larger than 1200 (significant change while charging is stopped)
        public void PowerConsumptionOptimizer_RefreshVehicleChargeState_IsTrue1(int previousNetPowerProduction, int netPowerProduction, string chargingState)
        {
            //arrange
            var chargeState = new TeslaAPI.Models.Vehicles.ChargeState();
            chargeState.ChargerVoltage = 240;
            chargeState.ChargingState = chargingState;
            chargeState.BatteryLevel = 50;

            var vehicle = new Vehicle { Name = "test vehicle", Id = "123456", ChargeState = chargeState, IsPriority = true };

            //act
            _consumptionOptimizer.RefreshVehicleChargeState(vehicle, previousNetPowerProduction, netPowerProduction);

            //assert
            Assert.True(vehicle.RefreshChargeState);
        }

        [Theory]
        //IsPriority charging vehicle is true for all tests
        [InlineData(1000, 800, "Charging")] // negative diff less than voltage; charging
        [InlineData(1000, 1200, "Charging")] // positive diff less than voltage; charging
        [InlineData(2100, 1900, "Stopped")] // negative diff less than voltage and can support 5 amps /w buffer; charging stopped
        [InlineData(1600, 1800, "Stopped")] // positive diff less than voltage and can support 5 amps /w buffer; charging stopped
        [InlineData(1200, 1000, "Stopped")] // negative diff larger than voltage but cannot support 5 amps /w buffer; charging stopped

        public void PowerConsumptionOptimizer_RefreshVehicleChargeState_IsFalse(int previousNetPowerProduction, int netPowerProduction, string chargingState)
        {
            //arrange
            var chargeState = new TeslaAPI.Models.Vehicles.ChargeState();
            chargeState.ChargerVoltage = 240;
            chargeState.ChargingState = chargingState;
            chargeState.BatteryLevel = 50;

            var vehicle = new Vehicle { Name = "test vehicle", Id = "123456", ChargeState = chargeState, IsPriority = true };

            //act
            _consumptionOptimizer.RefreshVehicleChargeState(vehicle, previousNetPowerProduction, netPowerProduction);

            //assert
            Assert.False(vehicle.RefreshChargeState);
        }


        [Theory]
        [InlineData(49, 80, false, 60, 100)] // test priority is given to vehicle below 50 even if difference between battery level and chargeLimitStateOfCharge is larger
        [InlineData(50, 80, false, 60, 70)] // test changes priority to vehicle with larger gap bettween battery level and chargeLimitStateOfCharge when both 50% or greater charge
        [InlineData(40, 100, false, 40, 80)] // test changes priority to vehicle with larger gap bettween battery level and chargeLimitStateOfCharge when both below 50% charge
        [InlineData(40, 80, true, 40, 80)] // test that priority stays with current IsPrioriy vehicle when two vehicles are otherwise equal priority
        [InlineData(40, 50, false, 40, 40)] // one vehicle has chargeLimitStateOfCharge < 40
        public void PowerConsumptionOptimizer_DetermineChargingPriority_IsVehicle1(int v1_batteryLevel, int v1_chargeLimitStateOfCharge, bool v1_isPriority, int v2_batteryLevel, int v2_chargeLimitStateOfCharge)
        {
            //arrange
            //List<Vehicle> vehicles = new();

            var v1 = new Vehicle { Name = "test vehicle 1", Id = "1" };
            var v2 = new Vehicle { Name = "test vehicle 2", Id = "2" };

            v1.ChargeState = new TeslaAPI.Models.Vehicles.ChargeState();
            v1.ChargeState.BatteryLevel = v1_batteryLevel;
            v1.ChargeState.ChargeLimitStateOfCharge = v1_chargeLimitStateOfCharge;
            v1.IsPriority = v1_isPriority;

            v2.ChargeState = new TeslaAPI.Models.Vehicles.ChargeState();
            v2.ChargeState.BatteryLevel = v2_batteryLevel;
            v2.ChargeState.ChargeLimitStateOfCharge = v2_chargeLimitStateOfCharge;

            _consumptionOptimizer.vehicles.Add(v1);
            _consumptionOptimizer.vehicles.Add(v2);

            //act
            _consumptionOptimizer.DetermineChargingPriority();

            //assert
            Assert.True(v1.IsPriority);
            Assert.False(v2.IsPriority);
        }

        [Theory]
        [InlineData(60, 100, false, 49, 55)] // test priority is given to vehicle below 50 even if limit and current favors other car
        [InlineData(40, 50, false, 40, 60)] // one vehicle has chargeLimitStateOfCharge < 40
        public void PowerConsumptionOptimizer_DetermineChargingPriority_IsVehicle2(int v1_batteryLevel, int v1_chargeLimitStateOfCharge, bool v1_isPriority, int v2_batteryLevel, int v2_chargeLimitStateOfCharge)
        {
            //arrange
            List<Vehicle> vehicles = new();

            var v1 = new Vehicle { Name = "test vehicle 1", Id = "1" };
            var v2 = new Vehicle { Name = "test vehicle 2", Id = "2" };

            v1.ChargeState = new TeslaAPI.Models.Vehicles.ChargeState();
            v1.ChargeState.BatteryLevel = v1_batteryLevel;
            v1.ChargeState.ChargeLimitStateOfCharge = v1_chargeLimitStateOfCharge;
            v1.IsPriority = v1_isPriority;

            v2.ChargeState = new TeslaAPI.Models.Vehicles.ChargeState();
            v2.ChargeState.BatteryLevel = v2_batteryLevel;
            v2.ChargeState.ChargeLimitStateOfCharge = v2_chargeLimitStateOfCharge;

            _consumptionOptimizer.vehicles.Add(v1);
            _consumptionOptimizer.vehicles.Add(v2);

            //act
            _consumptionOptimizer.DetermineChargingPriority();

            //assert
            Assert.True(v2.IsPriority);
            Assert.False(v1.IsPriority);
        }

        [Theory]
        [InlineData("Disconnected")]
        [InlineData("Complete")]
        //Vehicle 1 would have priority, but due to a "ChargingState" of Disconnected or Complete it does not
        public void PowerConsumptionOptimizer_DetermineChargingPriority_IsVehicle2_V1ChargeState(string v1ChargeState)
        {
            //arrange
            List<Vehicle> vehicles = new();

            var v1 = new Vehicle { Name = "test vehicle 1", Id = "1" };
            var v2 = new Vehicle { Name = "test vehicle 2", Id = "2" };

            v1.ChargeState = new TeslaAPI.Models.Vehicles.ChargeState();
            v1.ChargeState.BatteryLevel = 50;
            v1.ChargeState.ChargeLimitStateOfCharge = 99;
            v1.IsPriority = true;
            v1.ChargeState.ChargingState = v1ChargeState;

            v2.ChargeState = new TeslaAPI.Models.Vehicles.ChargeState();
            v2.ChargeState.BatteryLevel = 90;
            v2.ChargeState.ChargeLimitStateOfCharge = 92;

            _consumptionOptimizer.vehicles.Add(v1);
            _consumptionOptimizer.vehicles.Add(v2);

            //act
            _consumptionOptimizer.DetermineChargingPriority();

            //assert
            Assert.True(v2.IsPriority);
            Assert.False(v1.IsPriority);
        }

        [Fact]
        public void PowerConsumptionOptimizer_GetPriorityVehicle_IsNull()
        {
            //arrange
            List<Vehicle> vehicles = new();

            var v1 = new Vehicle { Name = "test vehicle 1", Id = "1" };
            var v2 = new Vehicle { Name = "test vehicle 2", Id = "2" };

            v1.IsPriority = false;
            v2.IsPriority = false;

            _consumptionOptimizer.vehicles.Add(v1);
            _consumptionOptimizer.vehicles.Add(v2);

            //act
            var vehicle = _consumptionOptimizer.GetPriorityVehicle();

            //assert
            Assert.Null(vehicle);
        }

        [Theory]
        [InlineData(true, false, "1")]  // vehicle 1 is priority
        [InlineData(false, true, "2")] // vehicle 2 is priority
        public void PowerConsumptionOptimizer_GetPriorityVehicle(bool v1_priority, bool v2_priority, string expected)
        {
            //arrange
            List<Vehicle> vehicles = new();

            var v1 = new Vehicle { Name = "test vehicle 1", Id = "1" };
            var v2 = new Vehicle { Name = "test vehicle 2", Id = "2" };

            v1.IsPriority = v1_priority;
            v2.IsPriority = v2_priority;

            _consumptionOptimizer.vehicles.Add(v1);
            _consumptionOptimizer.vehicles.Add(v2);

            //act
            var vehicle = _consumptionOptimizer.GetPriorityVehicle();

            //assert
            Assert.Equal(expected, vehicle.Id);
        }
    }
}
