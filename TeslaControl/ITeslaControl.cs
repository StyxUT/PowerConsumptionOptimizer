﻿using TeslaAPI.Models.Response;
using TeslaAPI.Models.Vehicles;

namespace TeslaControl
{
    public interface ITeslaControl
    {
        Task<CommandResponse> ChargeStartAsync(string vehicleId);
        Task<CommandResponse> ChargeStopAsync(string vehicleId);
        Task<List<Vehicle>> GetAllVehiclesAsync();
        Task<ChargeState> GetVehicleChargeStateAsync(string vehicleId);
        Task<CommandResponse> SetChargingAmpsAsync(string vehicleId, int chargingAmps);
    }
}