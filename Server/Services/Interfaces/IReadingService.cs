using Server.DTOs;
using Server.Models;

namespace Server.Services.Interfaces
{
    public interface IReadingService
    {
        Reading StoreReading(StoreReadingDto reading);
        IEnumerable<Reading> GetAllReadings();
        IEnumerable<Reading> GetReadingsBySensor(int sensorId);
    }
}