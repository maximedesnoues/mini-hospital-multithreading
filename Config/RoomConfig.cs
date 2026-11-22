using Raylib_cs;

public enum RoomType
{
    Generalist,
    Radiology,
    Psychiatry
}

namespace MiniHospital.Config
{
    public class RoomConfig
    {
        public string Name { get; }
        public RoomType Type { get; }
        public Slot Staff { get; }
        public Slot Patient { get; }

        public Rectangle ClosedArea { get; }

        public RoomConfig(string name, RoomType type, Slot staff, Slot patient, Rectangle closedArea)
        {
            Name = name;
            Type = type;
            Staff = staff;
            Patient = patient;
            ClosedArea = closedArea;
        }
    }
}