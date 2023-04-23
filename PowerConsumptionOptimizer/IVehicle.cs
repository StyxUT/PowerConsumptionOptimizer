namespace PowerConsumptionOptimizer
{
    public interface IVehicle
    {
        TeslaAPI.Models.Vehicles.ChargeState ChargeState { get; set; }
        string Id { get; init; }
        bool IsPriority { get; set; }
        string Name { get; init; }
        bool RefreshChargeState { get; set; }
        bool Equals(object? obj);
        bool Equals(Vehicle? other);
        int GetHashCode();
        string ToString();
    }
}