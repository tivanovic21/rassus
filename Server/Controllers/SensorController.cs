using Microsoft.AspNetCore.Mvc;
using Server.DTOs;
using Server.Models;
using Server.Services.Interfaces;

namespace Server.Controllers
{
    [ApiController] 
    [Route("api/[controller]")]
    public class SensorController : ControllerBase
    {
        private readonly ISensorService _sensorService;

        public SensorController(ISensorService sensorService)
        {
            _sensorService = sensorService;
        }

        [HttpGet("[action]")]
        public IActionResult GetAllSensors()
        {
            var sensors = _sensorService.GetAllSensors();
            return Ok(sensors);
        }
        
        [HttpGet("[action]/{id}")]
        public IActionResult GetNearestSensor(int id)
        {
            var sensor = _sensorService.FindNearestSensor(id);
            if (sensor != null)
                return Ok(sensor);
            return NotFound("Senzor nije pronađen");
        }

        [HttpPost("[action]")]
        public IActionResult RegisterSensor([FromBody] RegisterSensorDto model)
        {
            var sensor = _sensorService.RegisterSensor(model);
            if (sensor != null)
                return Ok(sensor);
            return BadRequest("Neuspješna registracija senzora");
        }
    }
}