using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace PowerProduction.Tests
{
    public class EnphaseLocalTests
    {

        private readonly EnphaseLocal _enphaseLocal;
        private readonly Mock<ILogger<EnphaseLocal>> _loggerMock;
        private readonly HttpClient _httpClient;

        public EnphaseLocalTests()
        {
            _loggerMock = new Mock<ILogger<EnphaseLocal>>();
            _enphaseLocal = new EnphaseLocal(_loggerMock.Object);
            _httpClient = new HttpClient();
        }


        [Fact]
        public async Task GetMeterDataAsync_ResultIsNotNull()
        {
            // Arrange
            _enphaseLocal.GetType().GetProperty("client")?.SetValue(_enphaseLocal, _httpClient);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("Applicaiton/json"));

            // Act
            var result = await _enphaseLocal.GetMeterDataAsync();

            // Assert
            Assert.NotNull(result.ActiveCount);
        }

        //[Fact]
        //public void EnphaseLocal_NetConsumption_Is1019()
        //{
        //    //arrange
        //    const string testJson = @"{'production':[{'type':'inverters','activeCount':23,'readingTime':1654480173,'wNow':260,'whLifetime':4691117},{'type':'eim','activeCount':1,'measurementType':'production','readingTime':1654480400,'wNow':201.1,'whLifetime':451318.976,'varhLeadLifetime':0.0,'varhLagLifetime':112369.264,'vahLifetime':512104.485,'rmsCurrent':4.177,'rmsVoltage':242.987,'reactPwr':446.336,'apprntPwr':507.627,'pwrFactor':0.42,'whToday':38740.976,'whLastSevenDays':325003.976,'vahToday':42960.485,'varhLeadToday':0.0,'varhLagToday':9135.264}],'consumption':[{'type':'eim','activeCount':1,'measurementType':'total-consumption','readingTime':1654480400,'wNow':1220.28,'whLifetime':226328.556,'varhLeadLifetime':105455.159,'varhLagLifetime':80189.408,'vahLifetime':472121.892,'rmsCurrent':12.934,'rmsVoltage':243.028,'reactPwr':-828.046,'apprntPwr':1574.831,'pwrFactor':0.78,'whToday':24648.556,'whLastSevenDays':157628.556,'vahToday':31397.892,'varhLeadToday':15135.159,'varhLagToday':4.408},{'type':'eim','activeCount':1,'measurementType':'net-consumption','readingTime':1654480400,'wNow':1019.18,'whLifetime':0.0,'varhLeadLifetime':105455.159,'varhLagLifetime':-32179.856,'vahLifetime':472121.892,'rmsCurrent':8.757,'rmsVoltage':243.007,'reactPwr':-381.71,'apprntPwr':2128.083,'pwrFactor':0.48,'whToday':0,'whLastSevenDays':0,'vahToday':0,'varhLeadToday':0,'varhLagToday':0}],'storage':[{'type':'acb','activeCount':0,'readingTime':0,'wNow':0,'whNow':0,'state':'idle'}]}";

        //    EnphaseLocal enphaseLocal = new EnphaseLocal(NullLogger<EnphaseLocal>.Instance);

        //    Type type = typeof(EnphaseLocal);
        //    MethodInfo methodInfo = type.GetMethod("ParseNetConsumption", BindingFlags.NonPublic | BindingFlags.Instance);

        //    //act
        //    var netConsumption = (EnphaseLocal.NetConsumption)methodInfo.Invoke(enphaseLocal, new object[] { testJson });

        //    //assert
        //    Assert.Equal(1019.18, netConsumption.Watts);
        //}

        [Fact]
        public void EnphaseLocal_GetNetPowerProduction_IsNotNull()
        {
            _enphaseLocal.GetType().GetProperty("client")?.SetValue(_enphaseLocal, _httpClient);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("Applicaiton/json"));

            double? netPowerProduction = _enphaseLocal.GetNetPowerProduction();

            Assert.NotNull(netPowerProduction);
        }
    }
}