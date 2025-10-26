using Server.Models;

namespace Server.Repositories.Interfaces
{
    public interface IReadingRepository
    {
        Reading Add(Reading reading);
        IEnumerable<Reading> GetAll();
        IEnumerable<Reading> GetBySensorId(int sensorId);
    }
}