using ConfigurationSettings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace PowerConsumptionOptimizer.Tests
{
    public class HelperTests
    {
        private static IConfigurationBuilder? builder;
        private static HelperSettings helperSettings;
        private readonly Mock<ILogger<ConsumptionOptimizer>> _loggerMock;


        public HelperTests()
        {
            builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true);
            IConfigurationRoot build = builder.Build();

            helperSettings = new HelperSettings();
            build.GetSection("Helpers").Bind(helperSettings);

            _loggerMock = new Mock<ILogger<ConsumptionOptimizer>>();
        }

        [Fact]
        public void Helpers_Normalize_CanInvert()
        {
            //arrange

            //act
            double normalizedValue = Helpers.Normalize(75, 0, 100, true);
            normalizedValue = System.Math.Round(normalizedValue, 5);

            //assert
            Assert.Equal(-0.84828, normalizedValue);
        }

        [Theory]
        [InlineData(1, 5, -3128, "Charging", 0)] // negative power production, charging
        [InlineData(245, 5, 700, "Stopped", 0)] // slightly positive power production, not charging
        [InlineData(240, 5, 2000, "Stopped", 6)] // positive power production, not charging
        [InlineData(245, 6, -500, "Charging", 1)] // negative power production, charging 
        [InlineData(230, 5, 500, "Charging", 5)] // negative power production, charging 
        [InlineData(245, 6, 750, "Charging", 7)] // slightly positive power production, charging
        [InlineData(120, 7, 750, "Charging", 9)] // 120 volts, positive power production, charging
        [InlineData(120, 7, 750, "Disconnected", 0)] // 120 volts, positive power production, disconnected
        [InlineData(240, 5, 2000, "Complete", 0)] // positive power production, charging complete
        [InlineData(240, 5, 220000, "Stopped", 48)] // massivly positive power production, charging is capped at 48 amps
        [InlineData(240, 5, 2000, "some unrecognized charging status", 0)] // positive power production, an unrecognized charging status
        [InlineData(240, 7, 750, "Charging", 0, false)] // 240 volts, positive power production, charging, not the charging priority
        [InlineData(240, 7, 750, "Stopped", 10, true, 10)] // 240 volts, positive power production, stopped, charging priority, below ChargeOverridePercentage

        public void Helpers_CalculateDesiredAmps(int currentVoltage, int currentAmps, double netPowerProduction, string chargingState, int correctDesiredAmps, bool isPriority = true, int batteryLevel = 50)
        {
            //arrange
            Vehicle vehicle = new Vehicle { Name = "test vehicle", Id = "123456" };
            vehicle.ChargeState = new TeslaAPI.Models.Vehicles.ChargeState();
            vehicle.ChargeState.ChargerVoltage = currentVoltage;
            vehicle.ChargeState.ChargeAmps = currentAmps;
            vehicle.ChargeState.ChargingState = chargingState;
            vehicle.ChargeState.BatteryLevel = batteryLevel;
            vehicle.IsPriority = isPriority;

            helperSettings.WattBuffer = 500;
            helperSettings.ChargeOverrideAmps = 10;
            helperSettings.ChargeOverridePercenage = 20;

            //act
            var calculatedDesiredAmps = Helpers.CalculateDesiredAmps(helperSettings, vehicle, netPowerProduction);

            //assert
            Assert.Equal(correctDesiredAmps, calculatedDesiredAmps);
        }
    }
}