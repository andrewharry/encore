using Encore.SampleWebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Encore.SampleWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IWeatherForecastService weatherService;

        public WeatherForecastController(IWeatherForecastService weatherService)
        {
            this.weatherService = weatherService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return weatherService.Get();
        }
    }
}