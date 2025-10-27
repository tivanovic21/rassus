using Microsoft.AspNetCore.Mvc;
using Server.DTOs;
using Server.Services.Interfaces;

namespace Server.Controllers
{
    [ApiController] 
    [Route("api/[controller]")]
    public class ReadingController : ControllerBase
    {
        private readonly IReadingService _readingService;
        private readonly ISensorService _sensorService;

        public ReadingController(IReadingService readingService, ISensorService sensorService)
        {
            _readingService = readingService;
            _sensorService = sensorService;
        }

        [HttpPost("[action]/{sensorId}")]
        public IActionResult StoreReading(int sensorId, [FromBody] StoreReadingDto model)
        {
            var sensor = _sensorService.GetSensorById(sensorId);
            if (sensor == null)
                return NoContent();

            model.SensorId = sensorId;
            var reading = _readingService.StoreReading(model);
            if (reading != null)
                return Ok(reading);
            return BadRequest("Neuspješno spremanje očitanja");
        }

        [HttpGet("[action]")]
        public IActionResult GetAllReadings()
        {
            var readings = _readingService.GetAllReadings();
            return Ok(readings);
        }

        [HttpGet("[action]/{sensorId}")]
        public IActionResult GetReadingsBySensor(int sensorId)
        {
            var readings = _readingService.GetReadingsBySensor(sensorId);
            if (readings == null || !readings.Any())
                return NoContent();
            return Ok(readings);
        }
    }
}