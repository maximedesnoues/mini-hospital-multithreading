using MiniHospital.Utils;

namespace MiniHospital.Config
{
    public class DayConfig
    {
        public int ReceptionistCount { get; set; } = 3;
        public int GeneralistCount { get; set; } = 3;

        public bool RadiologyOpen { get; set; } = true;
        public bool PsychiatryOpen { get; set; } = true;

        public TimeRange PatientInterval { get; set; } = new(3f, 6f);
        public TimeRange ReceptionDuration { get; set; } = new(1.5f, 3f);
        public TimeRange GeneralistDuration { get; set; } = new(5f, 9f);
        public TimeRange SpecialistDuration { get; set; } = new(7f, 12f);
        public TimeRange PatientPatience { get; set; } = new(28f, 40f);

        public float DayDuration { get; set; } = 120f;
        public float DayTransitionDelay { get; set; } = 3f;
        public int OpeningHour { get; set; } = 8;
        public int ClosingHour { get; set; } = 18;

        public void RandomizeOpenServices()
        {
            ReceptionistCount = 1 + Random.Shared.Next(0, HospitalLayout.ReceptionStaff.Length);
            GeneralistCount = 1 + Random.Shared.Next(0, HospitalLayout.Rooms.Count(r => r.Type == RoomType.Generalist));
            RadiologyOpen = Random.Shared.Next() % 2 == 0;
            PsychiatryOpen = Random.Shared.Next() % 2 == 0;
        }
    }
}
