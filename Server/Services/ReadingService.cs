using Server.DTOs;
using Server.Models;
using Server.Repositories.Interfaces;
using Server.Services.Interfaces;

namespace Server.Services
{
    public class ReadingService : IReadingService
    {
        private readonly IReadingRepository _readingRepository;

        public ReadingService(IReadingRepository readingRepository)
        {
            _readingRepository = readingRepository;
        }

        public IEnumerable<Reading> GetAllReadings()
        {
            return _readingRepository.GetAll();
        }

        public IEnumerable<Reading> GetReadingsBySensor(int sensorId)
        {
            return _readingRepository.GetBySensorId(sensorId);
        }

        public Reading StoreReading(StoreReadingDto model)
        {
            var reading = new Reading
            {
                Temperature = model.Temperature,
                Humidity = model.Humidity,
                Pressure = model.Pressure,
                SO2 = model.SO2,
                CO = model.CO,
            };
            return _readingRepository.Add(reading);
        }
    }
}