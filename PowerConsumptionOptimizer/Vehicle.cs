namespace PowerConsumptionOptimizer
{
    public record Vehicle() : IVehicle
    {
        public string Name { get; init; }
        public string Id { get; init; }
        public TeslaAPI.Models.Vehicles.ChargeState ChargeState { get; set; }
        public bool IsPriority { get; set; }
        public bool RefreshChargeState { get; set; } = true;
    }
}
