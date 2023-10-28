using ConfigurationSettings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace PowerConsumptionOptimizer.Tests
{
    public class HelperTests
    {
        private static IConfigurationBuilder? builder;
        private readonly Mock<ILogger<ConsumptionOptimizer>> _loggerMock;
        private IOptionsSnapshot<HelperSettings> _helperSettings;


        public static IOptionsMonitor<T> CreateIOptionMonitorMock<T>(T value) where T : class, new()
        {
            var mock = new Mock<IOptionsMonitor<T>>();
            mock.Setup(m => m.CurrentValue).Returns(value);
            return mock.Object;
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
        [InlineData(270, 5, 100, "Stopped", 1)] // slightly positive power production, not charging
        [InlineData(240, 5, 2000, "Stopped", 9)] // positive power production, not charging
        [InlineData(245, 6, -500, "Charging", 4)] // negative power production, charging 
        [InlineData(230, 5, 500, "Charging", 8)] // 230 amps, positive power production, charging 
        [InlineData(245, 6, 750, "Charging", 10)] // slightly positive power production, charging
        [InlineData(120, 7, 750, "Charging", 15)] // 120 volts, positive power production, charging
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

            var _helperSettings = CreateIOptionMonitorMock(new HelperSettings()
            {
                WattBuffer = -250,
                DefaultChargerVoltage = 240,
                ChargeOverridePercenage = 20,
                ChargeOverrideAmps = 10,
                UTCOffset = -6
            });

            //act
            var calculatedDesiredAmps = Helpers.CalculateDesiredAmps(_helperSettings, vehicle, netPowerProduction);

            //assert
            Assert.Equal(correctDesiredAmps, calculatedDesiredAmps);
        }
    }
}