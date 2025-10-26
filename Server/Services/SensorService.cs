using Server.DTOs;
using Server.Models;
using Server.Repositories.Interfaces;
using Server.Services.Interfaces;

namespace Server.Services
{
    public class SensorService : ISensorService
    {
        private readonly ISensorRepository _sensorRepository;

        public SensorService(ISensorRepository sensorRepository)
        {
            _sensorRepository = sensorRepository;
        }

        public Sensor? FindNearestSensor(int sensorId)
        {
            // TODO: Logika
            throw new NotImplementedException();
        }

        public IEnumerable<Sensor> GetAllSensors()
        {
            return _sensorRepository.GetAll();
        }

        public Sensor? GetSensorById(int sensorId)
        {
            return _sensorRepository.GetById(sensorId);
        }

        public Sensor RegisterSensor(RegisterSensorDto model)
        {
            var newSensor = new Sensor
            {
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                Ip = model.Ip,
                Port = model.Port
            };

            return _sensorRepository.Add(newSensor);
        }
    }
}