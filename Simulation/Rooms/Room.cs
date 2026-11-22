using MiniHospital.Config;
using System.Threading;

namespace MiniHospital.Simulation.Rooms
{
    public class Room : ServicePoint
    {
        public RoomConfig Config { get; }

        public bool IsOpen { get; set; }

        public Mutex ConsultationMutex { get; } = new Mutex();

        public Room(RoomConfig config): base(config.Staff, config.Patient)
        {
            Config = config;
            IsOpen = false;
        }
    }
}
