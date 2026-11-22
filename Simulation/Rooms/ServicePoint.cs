using MiniHospital.Config;
using MiniHospital.Simulation.Characters;
using MiniHospital.Utils;
using System.Threading;

namespace MiniHospital.Simulation.Rooms
{
    public abstract class ServicePoint
    {
        private readonly object occupantLock = new();

        public Slot StaffSlot { get; }
        public Slot PatientSlot { get; }

        private Employee? staff;
        private Patient? occupant;

        public Employee? Staff
        {
            get => Volatile.Read(ref staff);
            set => Volatile.Write(ref staff, value);
        }

        public Patient? Occupant
        {
            get
            {
                lock (occupantLock)
                {
                    return occupant;
                }
            }
        }

        protected ServicePoint(Slot staffSlot, Slot patientSlot)
        {
            StaffSlot = staffSlot;
            PatientSlot = patientSlot;
        }

        public bool TryAssignPatient(Patient patient)
        {
            lock (occupantLock)
            {
                if (occupant != null)
                {
                    return false;
                }

                Chaos.ExecuteIfEnabled();

                occupant = patient;
                return true;
            }
        }

        public void AssignPatient(Patient patient)
        {
            lock (occupantLock)
            {
                Chaos.ExecuteIfEnabled();

                if (occupant != null)
                {
                    throw new InvalidOperationException("Poste déjà occupé");
                }

                Chaos.ExecuteIfEnabled();

                occupant = patient;
            }
        }

        public void ReleasePatient(Patient? patient)
        {
            lock (occupantLock)
            {
                if (occupant != patient)
                {
                    throw new InvalidOperationException("Libération d'un poste par le mauvais patient");
                }

                occupant = null;

                Chaos.ExecuteIfEnabled();
            }
        }
    }
}
