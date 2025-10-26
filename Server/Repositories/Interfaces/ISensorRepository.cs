using Server.Models;

namespace Server.Repositories.Interfaces
{
    public interface ISensorRepository
    {
        Sensor? GetById(int id);
        IEnumerable<Sensor> GetAll();
        Sensor Add(Sensor sensor);
        bool Exists(int id);

    }
}