using Server.DTOs;
using Server.Models;

namespace Server.Services.Interfaces
{
    public interface ISensorService
    {
        Sensor RegisterSensor(RegisterSensorDto model);
        IEnumerable<Sensor> GetAllSensors();
        Sensor? FindNearestSensor(int sensorId);
        Sensor? GetSensorById(int sensorId);
    }
}