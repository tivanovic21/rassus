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
            var currentSensor = _sensorRepository.GetById(sensorId);
            if (currentSensor == null)
                return null;

            var allSensors = _sensorRepository.GetAll()
                .Where(s => s.Id != sensorId)
                .ToList();

            if (allSensors.Count == 0)
                return null;

            Sensor? nearest = null;
            double minDistance = double.MaxValue;

            foreach (var sensor in allSensors)
            {
                double distance = CalculateHaversineDistance(
                    currentSensor.Latitude, currentSensor.Longitude,
                    sensor.Latitude, sensor.Longitude);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = sensor;
                }
            }

            return nearest;
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

        private static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // radius u km

            double dLat = DegreesToRadians(lat2 - lat1);
            double dLon = DegreesToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}