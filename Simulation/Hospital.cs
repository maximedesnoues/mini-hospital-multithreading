using MiniHospital.Config;
using MiniHospital.Rendering;
using MiniHospital.Simulation.Characters;
using MiniHospital.Simulation.Rooms;
using MiniHospital.Utils;
using System.Collections.Concurrent;
using System.Threading;

namespace MiniHospital.Simulation
{
    public class Hospital
    {
        //configuration du TP: valeur de 0 = mode classique, autrmement le numéro de la question permet de mettre les comportements dans le mode adapté à la question
        public static int CurrentQuestion { get; internal set; } = 5;

        //public pour l'affichage uniquement
        public List<Room> Rooms { get; private set; } = new List<Room>();

        private int phaseValue;

        public DayPhase Phase
        {
            get => (DayPhase)Volatile.Read(ref phaseValue);

            private set => Interlocked.Exchange(ref phaseValue, (int)value);
        }

        public HospitalStats DayStats { get; private set; } = new HospitalStats();
        public HospitalStats TotalStats { get; } = new HospitalStats();

        public int DayNumber { get; private set; }
        public int EmployeeCount { get; private set; }

        public float SpeedMultiplier { get; set; } = 1f;

        public TimeSpan ClockTime
        {
            get
            {
                if (dayConfig == null)
                    return TimeSpan.FromHours(8);

                float progress = Math.Clamp(
                    elapsedTimeForToday / Math.Max(1f, dayConfig.DayDuration),
                    0f, 1f
                );

                double minutes =
                    dayConfig.OpeningHour * 60.0
                    + progress * (dayConfig.ClosingHour - dayConfig.OpeningHour) * 60.0;

                return TimeSpan.FromMinutes(minutes);
            }
        }

        private float elapsedTimeSinceStart;

        public float ElapsedTimeSinceStart
        {
            get => Volatile.Read(ref elapsedTimeSinceStart);

            private set => Volatile.Write(ref elapsedTimeSinceStart, value);
        }

        public static bool CallBrainOnUpdate { get; set; }

        private List<Sprite> FurnituresSprites = new List<Sprite>();

        private readonly object charactersLock = new();
        private readonly object admissionLock = new();
        private List<Character> characters = new List<Character>();
        private readonly List<Thread> dayThreads = new();
        private readonly ConcurrentDictionary<Slot, byte> freeSeats = new();

        private List<Reception> receptions = new List<Reception>();
        private SemaphoreSlim? receptionSemaphore;
        private List<Employee> employeesInBus = new List<Employee>();

        private readonly object busLock = new();

        private Barrier? employeeBoardingBarrier;

        private int busDepartureAuthorized;
        private int employeesFinishedCount;

        //pour l'affichage
        public int EmployeesInBusCount
        {
            get
            {
                lock (busLock)
                {
                    return employeesInBus.Count;
                }
            }
        }


        public enum DayPhase
        {
            Running,
            Closing,
            BusLeaving,
            Finished
        }

        public Hospital()
        {
            Initialize();

            StartDay(new DayConfig());

        }

        public void Initialize()
        {
            Rooms = new List<Room>();
            foreach (RoomConfig config in HospitalLayout.Rooms)
            {
                Rooms.Add(new Room(config));
            }

            receptions = new List<Reception>();
            for (int i = 0; i < HospitalLayout.ReceptionStaff.Length; i++)
            {
                receptions.Add(new Reception(
                    HospitalLayout.ReceptionStaff[i],
                    HospitalLayout.ReceptionPatients[i]
                ));
            }

            freeSeats.Clear();

            foreach (Slot seat in HospitalLayout.WaitingSeats)
            {
                if (!freeSeats.TryAdd(seat, 0))
                {
                    throw new Exception("Une chaise est présente plusieurs fois dans la configuration");
                }
            }

            InitializeFurniture();
            Navigation.InitFullGraph();

            characters.Clear();
            ElapsedTimeSinceStart = 0;
            CallBrainOnUpdate = Hospital.CurrentQuestion == 0? true : false;
        }

        public void Update(float deltaTime)
        {
            deltaTime *= SpeedMultiplier;
            ElapsedTimeSinceStart += deltaTime;
            HandleDay(deltaTime);

            Character[] charactersSnapshot;

            lock (charactersLock)
            {
                charactersSnapshot = characters.ToArray();
            }

            foreach (Character character in charactersSnapshot)
            {
                character.Update(deltaTime);
            }
        }

        private void HandleDay(float deltaTime)
        {
            switch (Phase)
            {
                case DayPhase.Running:
                    //lock de l'horloge pour les questions 1 à 4
                    if (CurrentQuestion == 0 || Hospital.CurrentQuestion >= 5)
                        elapsedTimeForToday += deltaTime;

                    if (elapsedTimeForToday >= dayConfig!.DayDuration)
                    {
                        lock (admissionLock)
                        {
                            elapsedTimeForToday = dayConfig.DayDuration;
                            Phase = DayPhase.Closing;
                        }
                    }
                    else
                    {

                        if (dayConfig != null)
                        {
                            if (ElapsedTimeSinceStart >= nextArrivalTime)
                            {
                                SpawnPatient();
                                nextArrivalTime = ElapsedTimeSinceStart + dayConfig.PatientInterval.Sample();
                            }
                        }

                    }
                    break;

                case DayPhase.Closing:
                    if (EmployeesInBusCount == EmployeeCount && DayStats.Active == 0)
                    {
                        Phase = DayPhase.BusLeaving;
                        transitionTimer = 0f;
                    }
                    break;

                case DayPhase.BusLeaving:
                    transitionTimer += deltaTime;

                    if (transitionTimer >= dayConfig!.DayTransitionDelay)
                    {
                        Volatile.Write(ref busDepartureAuthorized, 1);
                    }

                    if (Volatile.Read(ref busDepartureAuthorized) == 1 && Volatile.Read(ref employeesFinishedCount) == EmployeeCount)
                    {
                        lock (busLock)
                        {
                            employeesInBus.Clear();
                        }

                        Phase = DayPhase.Finished;
                    }
                    break;

                case DayPhase.Finished:
                    DayConfig newday = new DayConfig();
                    newday.RandomizeOpenServices();
                    StartDay(newday);
                    break;
			}
        }

        public List<Sprite> GetRenderSprites()
        {
            var list = new List<Sprite>(FurnituresSprites);

            Character[] charactersSnapshot;

            lock (charactersLock)
            {
                charactersSnapshot = characters.ToArray();
            }

            foreach (Character character in charactersSnapshot)
            {
                character.GetRenderSprites(list);
            }

            return list;
        }

        private void InitializeFurniture()
        {
            FurnituresSprites = new List<Sprite>();
            foreach (SpriteConfig config in HospitalLayout.Furniture)
            {
                Sprite sprite = new Sprite(
                    TextureManager.Get(config.Texture),
                    config.Source,
                    config.Position,
                    config.Anchor
                );

                sprite.Layer = config.Layer;
                sprite.Scale = config.Scale;

                FurnituresSprites.Add(sprite);
            }
        }

        private DayConfig? dayConfig;
        private float nextArrivalTime;
        private float elapsedTimeForToday;
        private float transitionTimer;

        public Room? ChooseValidRoom(bool wantsSpecialist)
        {
            Room? assigned = null;
            Room[] specialists = Rooms.Where(r =>
                r.IsOpen && r.Staff != null && (r.Config.Type != RoomType.Generalist) == wantsSpecialist
            ).ToArray();

            if (specialists.Length > 0)
                assigned = specialists[Random.Shared.Next(specialists.Length)];
            return assigned;
        }

        private void FinishPreviousDay()
        {
            if (DayNumber == 0)
            {
                return;
            }

            if (Phase != DayPhase.Finished)
            {
                throw new InvalidOperationException("Impossible de démarrer une nouvelle journée avant la fin de la précédente");
            }

            foreach (Thread thread in dayThreads)
            {
                if (!thread.Join(TimeSpan.FromSeconds(10)))
                {
                    throw new InvalidOperationException($"Le thread {thread.ManagedThreadId} n'a pas terminé");
                }
            }

            dayThreads.Clear();

            employeeBoardingBarrier?.Dispose();
            employeeBoardingBarrier = null;

            receptionSemaphore?.Dispose();
            receptionSemaphore = null;
        }

        public void StartDay(DayConfig config)
        {
            FinishPreviousDay();

            lock (charactersLock)
            {
                if (characters.Count != 0)
                    throw new Exception("Des personnages sont encore presents");
            }
            if (DayNumber > 0 && freeSeats.Count != HospitalLayout.WaitingSeats.Length)
                throw new Exception("Certains sièges n'ont pas été libérés la veille");
            if(EmployeesInBusCount > 0)
                throw new Exception("Le bus n'est pas vide en début de journée");
            if (DayNumber > 0 && DayStats.Arrived != DayStats.Treated + DayStats.Untreated)
                throw new Exception("Statistiques de la veille incohérentes");

            if (DayNumber > 0)
            {
                foreach (Reception reception in receptions)
                {
                    if (reception.Staff != null || reception.Occupant != null)
                    {
                        throw new InvalidOperationException("Un guichet n'a pas été entièrement libéré la veille");
                    }
                }
            }

            dayConfig = config;

            DayNumber++;
            DayStats = new HospitalStats();

            elapsedTimeForToday = 0f;
            transitionTimer = 0f;
            nextArrivalTime = ElapsedTimeSinceStart + config.PatientInterval.Sample();
            EmployeeCount = 0;


            freeSeats.Clear();

            foreach (Slot seat in HospitalLayout.WaitingSeats)
            {
                if (!freeSeats.TryAdd(seat, 0))
                {
                    throw new Exception("Une chaise est présente plusieurs fois dans la configuration");
                }
            }

            // Déterminer les salles ouvertes.
            int generalistIndex = 0;
            foreach (Room room in Rooms)
            {
                if(room.Staff != null || room.Occupant != null)
                {
                    throw new Exception("Certaines salles n'ont pas été libérées la veille");
                }
                room.Staff = null;
                room.ReleasePatient(null);

                switch (room.Config.Type)
                {
                    case RoomType.Generalist:
                        room.IsOpen = generalistIndex < config.GeneralistCount;
                        generalistIndex++;
                        break;

                    case RoomType.Radiology:
                        room.IsOpen = config.RadiologyOpen;
                        break;

                    case RoomType.Psychiatry:
                        room.IsOpen = config.PsychiatryOpen;
                        break;
                }
            }

            // Créer les réceptionistes.
            for (int i = 0; i < config.ReceptionistCount; i++)
            {

                Reception reception = receptions[i];
                if (reception.Staff != null || reception.Occupant != null)
                {
                    throw new Exception("Certains accueils n'ont pas été libérées la veille");
                }
                reception.Staff = null;
                reception.ReleasePatient(null);

                Employee employee = new(
                    CharacterConfig.ReceptionitsSpriteIds[Random.Shared.Next(CharacterConfig.ReceptionitsSpriteIds.Length)],
                    reception,
                    this,
                    dayConfig.ReceptionDuration
                );

                reception.Staff = employee;
                employee.PlaceAt(reception.StaffSlot);
                lock (charactersLock)
                {
                    characters.Add(employee);
                }
                ++EmployeeCount;
            }

            // Créer un médecin par salle ouverte.
            foreach (Room room in Rooms)
            {
                if (!room.IsOpen)
                    continue;
                TimeRange duration = dayConfig.GeneralistDuration;
                switch(room.Config.Type)
                {
                    case RoomType.Generalist:
                        duration = dayConfig.GeneralistDuration;
                        break;
                    case RoomType.Psychiatry:
                        duration = dayConfig.SpecialistDuration;
                        break;
                    case RoomType.Radiology:
                        duration = dayConfig.SpecialistDuration;
                        break;
                }

                Employee employee = new(
                    CharacterConfig.DoctorSpriteIds[Random.Shared.Next(CharacterConfig.DoctorSpriteIds.Length)],
                    room,
                    this,
                    duration
                );

                room.Staff = employee;
                lock (charactersLock)
                {
                    characters.Add(employee);
                }
                ++EmployeeCount;
            }

            if (CurrentQuestion >= 5)
            {
                employeeBoardingBarrier = new Barrier(EmployeeCount);

                Volatile.Write(ref busDepartureAuthorized, 0);
                Interlocked.Exchange(ref employeesFinishedCount, 0);
            }

            Phase = DayPhase.Running;

            if (CurrentQuestion >= 3)
            {
                receptionSemaphore = new SemaphoreSlim(config.ReceptionistCount, Math.Max(1, config.ReceptionistCount));

                Employee[] employees;

                lock (charactersLock)
                {
                    employees = characters.OfType<Employee>().ToArray();
                }

                foreach (Employee employee in employees)
                {
                    dayThreads.Add(employee.StartBrainThread());
                }
            }
        }

        private void SpawnPatient()
        {
            DayStats.RegisterArrival();
            TotalStats.RegisterArrival();

            Patient patient = new(
                CharacterConfig.PatientSpriteIds[Random.Shared.Next(CharacterConfig.PatientSpriteIds.Length)],
                this,
                dayConfig!.PatientPatience.Sample());

            patient.PlaceAt(NavNode.Outside);

            lock (charactersLock)
            {
                characters.Add(patient);
            }

            dayThreads.Add(patient.StartBrainThread());
        }

        public Slot TakeFreeSeat()
        {
            foreach (Slot seat in freeSeats.Keys)
            {
                Chaos.ExecuteIfEnabled();

                if (freeSeats.TryRemove(seat, out _))
                {
                    return seat;
                }
            }

            return HospitalLayout.StandingWaitingSlots[Random.Shared.Next(HospitalLayout.StandingWaitingSlots.Length)];
        }

        public void ReleaseSeat(Slot seat)
        {
            if (seat.Pose != CharacterAction.Sitting)
            {
                return;
            }

            Chaos.ExecuteIfEnabled();

            if (!freeSeats.TryAdd(seat, 0))
            {
                throw new Exception("Un siège vient d'être libéré une 2e fois");
            }
        }

        public void RegisterEmployeeBoarding(Employee employee)
        {
            lock (busLock)
            {
                if (employeesInBus.Contains(employee))
                {
                    throw new Exception("Employé est déjà dans le bus");
                }

                lock (charactersLock)
                {
                    if (!characters.Remove(employee))
                    {
                        throw new Exception("Enregistrement de départ d'un employé non présent dans l'hopital");
                    }
                }

                employeesInBus.Add(employee);
            }
        }

        public void WaitForAllEmployeesAtBus()
        {
            if (employeeBoardingBarrier == null)
            {
                throw new InvalidOperationException("La barrière du bus n'est pas initialisée");
            }

            employeeBoardingBarrier.SignalAndWait();
        }

        public bool CanEmployeeLeaveBus => Volatile.Read(ref busDepartureAuthorized) == 1;

        public void RegisterEmployeeFinished()
        {
            int finished = Interlocked.Increment(ref employeesFinishedCount);

            if (finished > EmployeeCount)
            {
                throw new InvalidOperationException("Trop d'employés ont terminé leur journée");
            }
        }

        public void RegisterPatientDeparture(Patient patient, bool treated)
        {
            lock (charactersLock)
            {
                if (!characters.Remove(patient))
                {
                    throw new Exception("Enregistrement de départ d'un patient non présent dans l'hopital");
                }
            }

            DayStats.RegisterDeparture(treated);
            TotalStats.RegisterDeparture(treated);
        }

        public bool TryAcquireReceptionPermit()
        {
            return receptionSemaphore?.Wait(0) ?? false;
        }

        public void ReleaseReceptionPermit()
        {
            if (receptionSemaphore == null)
            {
                throw new InvalidOperationException("Le sémaphore des guichets n'est pas initialisé");
            }

            receptionSemaphore.Release();
        }

        public Reception? TryGetReception(Patient patient)
        {
            foreach (Reception candidate in receptions)
            {
                if (candidate.IsOpen && candidate.Staff != null && candidate.Staff.IsAvailable && candidate.TryAssignPatient(patient))
                {
                    return candidate;
                }
            }

            return null;
        }

        public Reception? TryReserveReception(Patient patient)
        {
            lock (admissionLock)
            {
                if (Phase != DayPhase.Running || !TryAcquireReceptionPermit())
                {
                    return null;
                }

                try
                {
                    Reception? reception = TryGetReception(patient);

                    if (reception == null)
                    {
                        ReleaseReceptionPermit();
                    }

                    return reception;
                }
                catch
                {
                    ReleaseReceptionPermit();
                    throw;
                }
            }
        }

        public bool TryReserveRoom(Patient patient, Room room)
        {
            lock (admissionLock)
            {
                if (Phase != DayPhase.Running || !room.IsOpen || room.Staff?.IsAvailable != true)
                {
                    return false;
                }

                if (!room.ConsultationMutex.WaitOne(0))
                {
                    return false;
                }

                bool assigned = false;

                try
                {
                    assigned = room.TryAssignPatient(patient);
                    return assigned;
                }
                finally
                {
                    if (!assigned)
                    {
                        room.ConsultationMutex.ReleaseMutex();
                    }
                }
            }
        }

        public bool TryStartHandlingPatient(Employee employee, Patient patient)
        {
            lock (admissionLock)
            {
                if (Phase != DayPhase.Running)
                {
                    return false;
                }

                employee.HandlePatient(patient);
                return true;
            }
        }
    }
}