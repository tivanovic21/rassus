using System.Collections.Concurrent;
using Server.Models;
using Server.Repositories.Interfaces;

namespace Server.Repositories
{
    public class ReadingRepository : IReadingRepository
    {
        private readonly ConcurrentDictionary<int, Reading> _readings = new();
        private int _nextId = 0;

        public Reading Add(Reading reading)
        {
            reading.Id = Interlocked.Increment(ref _nextId);
            _readings.TryAdd(reading.Id, reading);
            return _readings.GetValueOrDefault(reading.Id)!;
        }

        public IEnumerable<Reading> GetAll()
        {
            return _readings.Values.ToArray();
        }

        public IEnumerable<Reading> GetBySensorId(int sensorId)
        {
            return _readings.Values.Where(r => r.SensorId == sensorId).ToArray();
        }
    }
}