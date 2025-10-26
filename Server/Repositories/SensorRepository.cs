using System.Collections.Concurrent;
using Server.Models;
using Server.Repositories.Interfaces;

namespace Server.Repositories
{
    public class SensorRepository : ISensorRepository
    {
        private readonly ConcurrentDictionary<int, Sensor> _sensors = new();
        private int _nextId = 1;

        public Sensor Add(Sensor sensor)
        {
            sensor.Id = _nextId++;
            _sensors.TryAdd(sensor.Id, sensor);
            return sensor;
        }

        public bool Exists(int id)
        {
            return _sensors.ContainsKey(id);
        }

        public IEnumerable<Sensor> GetAll()
        {
            return _sensors.Values;
        }

        public Sensor? GetById(int id)
        {
            return _sensors.TryGetValue(id, out var sensor) ? sensor : null;
        }
    }
}