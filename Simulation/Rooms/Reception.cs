using MiniHospital.Config;

namespace MiniHospital.Simulation.Rooms
{
    public class Reception : ServicePoint
    {
        public bool IsOpen => Staff != null;

        public Reception(Slot staffSlot, Slot patientSlot) : base(staffSlot, patientSlot)
        {
        }
    }
}
