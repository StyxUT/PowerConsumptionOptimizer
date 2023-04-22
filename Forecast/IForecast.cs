namespace Forecast
{
    public interface IForecast
    {
        public double GetSolarIrradianceNextHour();
        public List<double> GetSolarIrradianceByHour();
    }
}
