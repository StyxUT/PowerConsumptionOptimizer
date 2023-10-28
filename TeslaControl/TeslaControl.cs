using Microsoft.Extensions.Logging;
using Polly;
using TeslaAPI;
using TeslaAPI.Models;
using TeslaAPI.Models.Response;
using TeslaAPI.Models.Vehicles;

namespace TeslaControl
{
    public class TeslaControl : ITeslaControl
    {
        private readonly ILogger<TeslaControl> _logger;
        private readonly ITeslaAPI _teslaAPI;
        private readonly HttpClient _client = new();

        private TeslaRefreshToken? _teslaRefreshToken;
        public TeslaAccessToken TeslaAccessToken = new();
        public TeslaBearerToken TeslaBearerToken = new();

        private readonly object tokenLock = new();

        //readonly RetryPolicy<HttpResponseMessage> _httpRetryPolicy;
        readonly IAsyncPolicy retryOnException;

        public string VehicleId;
        public string RefreshToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeslaControl"/> class.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="teslaClient">The TeslaAPI client.</param>
        /// <param name="refreshToken">The Tesla refresh token.</param>
        public TeslaControl(ILogger<TeslaControl> logger, ITeslaAPI teslaAPI, string refreshtoken)
        {
            _logger = logger;
            _teslaAPI = teslaAPI;
            _client.BaseAddress = new Uri(@"https://owner-api.teslamotors.com/api/1");
            _client.DefaultRequestHeaders.Add("User-Agent", "PCO");
            RefreshToken = refreshtoken;

            retryOnException = Policy.Handle<Exception>()
                .WaitAndRetryAsync(retryCount: 10,
                    sleepDurationProvider: (retry) => TimeSpan.FromSeconds(10 * retry),
                    onRetry: (ex, waitTime, retryNum, context) =>
                    {
                        _logger.LogDebug($"TeslaControl - Retry {retryNum} for {context.OperationKey} due to {ex.Message}\n\t\t next retry in {waitTime.TotalSeconds} seconds");

                        if (ex.Message.Contains("invalid bearer token"))
                        {
                            _logger.LogDebug($"TeslaControl - invalid bearer token");
                            ManageAccessToken(); 
                        }
                        else if (ex.Message.Contains("vehicle unavailable"))
                        { WakeUpVehicle(); }
                        else
                        {
                            _logger.LogDebug($"TeslaControl - unanticipated error");
                            ManageAccessToken(); 
                            WakeUpVehicle(); 
                        };
                    });

            ManageAccessToken();
        }

        private void ManageAccessToken()
        {
            lock (tokenLock)
            {
                _logger.LogDebug("TeslaControl - ManageAccessToken - Refreshing Tesla access token");
                _logger.LogDebug($"TeslaControl - ManageAccessToken - RefreshToken: ${RefreshToken}");
                _logger.LogDebug($"TeslaControl - ManageAccessToken - _client.BaseAddress: ${_client.BaseAddress}");

                _teslaRefreshToken = retryOnException.ExecuteAsync(action => _teslaAPI.RefreshTokenAsync(_client, RefreshToken), context: new Context("ManageAccessToken")).Result;

                TeslaAccessToken.AccessToken = _teslaRefreshToken.AccessToken;
                TeslaAccessToken.ExpiresIn = _teslaRefreshToken.ExpiresIn;
                _logger.LogDebug($"TeslaControl - ManageAccessToken - TeslaAccessToken.AccessToken: ${TeslaAccessToken.AccessToken}");

                TeslaBearerToken.AccessToken = _teslaRefreshToken.AccessToken;
                TeslaBearerToken.ExpiresIn = _teslaRefreshToken.ExpiresIn;
                _logger.LogDebug($"TeslaControl - ManageAccessToken - TeslaBearerToken.AccessToken: ${TeslaBearerToken.AccessToken}");

                // update authorization header
                _client.DefaultRequestHeaders.Remove("Authorization");
                _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {TeslaBearerToken.AccessToken}");
            }
        }

        private async void WakeUpVehicle()
        {
            _logger.LogDebug("TeslaControl - Waking up vehicle");
            var vehicle = retryOnException.ExecuteAsync(action => _teslaAPI.WakeUpAsync(_client, VehicleId), context: new Context("WakeUpVehicle")).Result;
        }

        public async Task<List<Vehicle>> GetAllVehiclesAsync()
        {
            return await retryOnException.ExecuteAsync<List<Vehicle>>(async action => await _teslaAPI.GetAllVehiclesAsync(_client), context: new Context("GetAllVehiclesAsync"));
        }

        public async Task<ChargeState> GetVehicleChargeStateAsync(string vehicleId)
        {
            VehicleId = vehicleId;
            var result = await retryOnException.ExecuteAsync<VehicleData>(async action => await _teslaAPI.GetVehicleDataAsync(_client, vehicleId), context: new Context("GetVehicleChargeStateAsync"));
            return result.ChargeState;
        }

        public async Task<CommandResponse> ChargeStartAsync(string vehicleId)
        {
            VehicleId = vehicleId;
            return await retryOnException.ExecuteAsync(async action => await _teslaAPI.ChargeStartAsync(_client, vehicleId), context: new Context("ChargeStartAsync"));
        }

        public async Task<CommandResponse> ChargeStopAsync(string vehicleId)
        {
            VehicleId = vehicleId;
            return await retryOnException.ExecuteAsync(async action => await _teslaAPI.ChargeStopAsync(_client, vehicleId), context: new Context("ChargeStopAsync"));
        }

        public async Task<CommandResponse> SetChargingAmpsAsync(string vehicleId, int chargingAmps)
        {
            VehicleId = vehicleId;
            return await retryOnException.ExecuteAsync(async action => await _teslaAPI.SetChargingAmpsAsync(_client, vehicleId, chargingAmps), context: new Context("SetChargingAmpsAsync"));
        }
    }
}