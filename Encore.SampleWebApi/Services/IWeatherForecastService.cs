namespace Encore.SampleWebApi.Services
{
    public interface IWeatherForecastService
    {
        IEnumerable<WeatherForecast> Get();
    }
}