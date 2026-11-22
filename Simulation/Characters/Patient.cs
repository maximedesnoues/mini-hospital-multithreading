using MiniHospital.Config;
using MiniHospital.Rendering;
using MiniHospital.Simulation.Rooms;
using MiniHospital.Utils;
using System.Collections.Concurrent;

namespace MiniHospital.Simulation.Characters
{
    public class Patient : Character
    {
        public Room? AssignedRoom { get; private set; }
        public Reception? AssignedReception { get; private set; }


        private Hospital hospital;
        private Step step = Step.Arriving;

        private GaugeSprite gaugeSprite;
        private bool wasTreated;
        private float waitingStartedAt;
        private Slot? waitingSeat;
        private Room? consultationMutexRoom;
        private float maxPatience;

        private readonly ConcurrentQueue<Room?> registrationNotifications = new();

        public void NotifyRegistrationFinished(Room? room)
        {
            registrationNotifications.Enqueue(room);
        }

        private readonly ConcurrentQueue<Room?> consultationNotifications = new();

        public void NotifyConsultationFinished(Room? specialist)
        {
            consultationNotifications.Enqueue(specialist);
        }

        public Patient(int spriteIndex, Hospital hospital, float maxPatience) : base(spriteIndex)
        {
            this.hospital = hospital;
            SetRenderScale(CharacterConfig.PatientScale);
            gaugeSprite = new GaugeSprite();
            this.maxPatience = maxPatience;
        }

        private enum Step
        {
            Arriving,
            GoingToReception,
            AtReception,
            GoingToWaiting,
            WaitingForReception,
            WaitingForRoom,
            GoingToRoom,
            InConsultation,
            Leaving,
            Finished
        }

        protected override bool IsBrainFinished => step == Step.Finished;

        public override void Update(float deltaTime)
        {
            lock (characterLock)
            {
                base.Update(deltaTime);
                gaugeSprite.Position = position + CharacterConfig.PatienceGaugePosition;
            }
        }

        public override void GetRenderSprites(List<Sprite> list)
        {
            lock (characterLock)
            {
                base.GetRenderSprites(list);

                if (step == Step.WaitingForReception || step == Step.WaitingForRoom)
                    list.Add(gaugeSprite.Copy());
            }
        }

        private void SetCurrentStep(Step next)
        {
            Chaos.ExecuteIfEnabled();
            step = next;

            if(Hospital.CurrentQuestion != 0)
            {
                //cas spéciaux pour les exercices
                if (Hospital.CurrentQuestion == 1 && next == Step.AtReception ||
                    Hospital.CurrentQuestion == 2 && next == Step.WaitingForReception ||
                    Hospital.CurrentQuestion == 3 && next == Step.WaitingForRoom)
                {
                    StartLeaving(false);
                }

            }
        }

        public void RegistrationFinished(Room? room)
        {
            if (step != Step.AtReception || AssignedReception == null)
            {
                throw new InvalidOperationException("Enregistrement termine hors de l'accueil");
            }

            AssignedRoom = room;

            AssignedReception.ReleasePatient(this);
            AssignedReception = null;

            hospital.ReleaseReceptionPermit();

            if (AssignedRoom == null || hospital.Phase != Hospital.DayPhase.Running)
            {
                StartLeaving(false);
            }
            else
            {
                GoToWaiting();
            }
        }

        private void ReleaseConsultationRoom()
        {
            if (consultationMutexRoom == null)
            {
                return;
            }

            Room room = consultationMutexRoom;

            room.ReleasePatient(this);
            room.ConsultationMutex.ReleaseMutex();

            consultationMutexRoom = null;
        }

        public void ConsultationFinished(Room? specialist)
        {
            if (step != Step.InConsultation || AssignedRoom == null || consultationMutexRoom != AssignedRoom)
            {
                throw new InvalidOperationException("Consultation terminee hors consultation ou sans mutex");
            }

            ReleaseConsultationRoom();
            AssignedRoom = null;

            if (specialist != null && hospital.Phase == Hospital.DayPhase.Running)
            {
                AssignedRoom = specialist;
                GoToWaiting();
            }
            else
            {
                // Conserve la distinction entre consultation terminée et orientation non réalisée.
                StartLeaving(specialist == null);
            }
        }

        private bool TryGotoReception()
        {
            Reception? reception = hospital.TryReserveReception(this);

            if (reception == null)
            {
                return false;
            }

            AssignedReception = reception;
            ReleaseWaitingSeat();
            MoveTo(reception.PatientSlot);
            SetBubbletype(BubbleType.Question);
            SetCurrentStep(Step.GoingToReception);

            return true;
        }

        private void GoToWaiting()
        {
            waitingSeat = hospital.TakeFreeSeat();
            MoveTo(waitingSeat);
            SetBubbletype(BubbleType.Wait);
            SetCurrentStep(Step.GoingToWaiting);
        }

        private void ReleaseWaitingSeat()
        {
            if (waitingSeat == null)
                return;
            hospital.ReleaseSeat(waitingSeat);
            waitingSeat = null;
        }

        private bool WaitOrGiveUp()
        {
            float remaining = 1f -
                (hospital.ElapsedTimeSinceStart - waitingStartedAt) / maxPatience;
            gaugeSprite.Value = Math.Clamp(remaining, 0f, 1f);
            SetBubbletype(remaining < 0.25f ? BubbleType.Angry : BubbleType.Wait);

            if (remaining > 0f)
                return false;

            StartLeaving(false);
            return true;
        }

        private void StartLeaving(bool treated)
        {
            wasTreated = treated;
            ReleaseWaitingSeat();

            if (AssignedReception != null)
            {
                if (AssignedReception.Occupant == this)
                {
                    AssignedReception.ReleasePatient(this);
                    hospital.ReleaseReceptionPermit();
                }

                AssignedReception = null;
            }

            if (consultationMutexRoom != null)
            {
                if (AssignedRoom != consultationMutexRoom)
                {
                    throw new InvalidOperationException("Le patient détient le mutex d'un autre cabinet");
                }

                ReleaseConsultationRoom();
            }
            else if (AssignedRoom?.Occupant == this)
            {
                throw new InvalidOperationException("Le patient occupe un cabinet sans détenir son mutex");
            }

            AssignedRoom = null;

            SetBubbletype(treated ? BubbleType.Done : BubbleType.Angry);
            MoveTo(NavNode.Outside);
            SetCurrentStep(Step.Leaving);
        }

        protected override void Brain()
        {
            if (hospital.Phase != Hospital.DayPhase.Running &&
                step != Step.InConsultation && step != Step.AtReception &&
                step != Step.Leaving && step != Step.Finished)
            {
                StartLeaving(false);
                return;
            }
            
            switch (step)
            {
                case Step.Arriving:
                    //cas spéciaux pour les exercices
                    if (Hospital.CurrentQuestion == 1)
                    {
                        MoveTo(HospitalLayout.ReceptionPatients[0]);
                        SetCurrentStep(Step.GoingToReception);
                    }
                    else if (Hospital.CurrentQuestion == 2)
                    {
                        GoToWaiting();
                    }
                    else if (!TryGotoReception())
                    {
                        GoToWaiting();
                    }
                    break;

                case Step.GoingToReception:
                    if (!IsMoving)
                    {
                        if (AssignedReception == null || AssignedReception.Occupant != this)
                        {
                            throw new InvalidOperationException("Début d'enregistrement sans guichet correctement réservé");
                        }

                        if (hospital.Phase != Hospital.DayPhase.Running)
                        {
                            StartLeaving(false);
                            break;
                        }

                        Employee? receptionist = AssignedReception.Staff;

                        if (receptionist == null)
                        {
                            if (hospital.Phase != Hospital.DayPhase.Running)
                            {
                                StartLeaving(false);
                                break;
                            }

                            throw new InvalidOperationException("Début d'enregistrement sans secrétaire disponible");
                        }

                        SetCurrentStep(Step.AtReception);

                        if (step == Step.AtReception)
                        {
                            bool started;

                            try
                            {
                                started = hospital.TryStartHandlingPatient(receptionist, this);
                            }
                            catch
                            {
                                StartLeaving(false);
                                throw;
                            }

                            if (!started)
                            {
                                StartLeaving(false);
                            }
                        }
                    }
                    break;

                case Step.AtReception:
                    if (registrationNotifications.TryDequeue(out Room? assignedRoom))
                    {
                        RegistrationFinished(assignedRoom);
                    }
                    break;

                case Step.GoingToWaiting:
                    if (!IsMoving)
                    {
                        waitingStartedAt = hospital.ElapsedTimeSinceStart;
                        gaugeSprite.Value = 1f;
                        SetCurrentStep(AssignedRoom == null ? Step.WaitingForReception : Step.WaitingForRoom);
                    }
                    break;

                case Step.WaitingForReception:
                    if (!WaitOrGiveUp())
                        TryGotoReception();
                    break;

                case Step.WaitingForRoom:
                    if (WaitOrGiveUp())
                    {
                        break;
                    }

                    if (AssignedRoom != null)
                    {
                        Room room = AssignedRoom;

                        if (hospital.TryReserveRoom(this, room))
                        {
                            consultationMutexRoom = room;

                            try
                            {
                                ReleaseWaitingSeat();
                                MoveTo(room);
                                SetBubbletype(BubbleType.Health);
                                SetCurrentStep(Step.GoingToRoom);
                            }
                            catch
                            {
                                ReleaseConsultationRoom();
                                AssignedRoom = null;
                                throw;
                            }
                        }
                    }
                    break;

                case Step.GoingToRoom:
                    if (!IsMoving)
                    {
                        if (AssignedRoom == null || consultationMutexRoom != AssignedRoom || AssignedRoom.Occupant != this)
                        {
                            throw new InvalidOperationException("Début de consultation sans cabinet correctement réservé");
                        }

                        if (hospital.Phase != Hospital.DayPhase.Running)
                        {
                            StartLeaving(false);
                            break;
                        }

                        Employee? doctor = AssignedRoom.Staff;

                        if (doctor == null)
                        {
                            if (hospital.Phase != Hospital.DayPhase.Running)
                            {
                                StartLeaving(false);
                                break;
                            }

                            throw new InvalidOperationException("Début de consultation sans médecin disponible");
                        }

                        SetCurrentStep(Step.InConsultation);

                        bool started;

                        try
                        {
                            started = hospital.TryStartHandlingPatient(doctor, this);
                        }
                        catch
                        {
                            ReleaseConsultationRoom();
                            AssignedRoom = null;
                            throw;
                        }

                        if (!started)
                        {
                            StartLeaving(false);
                        }
                    }
                    break;

                case Step.InConsultation:
                    if (AssignedRoom == null || consultationMutexRoom != AssignedRoom || AssignedRoom.Occupant != this)
                    {
                        throw new InvalidOperationException("Patient en consultation sans cabinet correctement réservé");
                    }

                    if (consultationNotifications.TryDequeue(out Room? specialist))
                    {
                        ConsultationFinished(specialist);
                    }
                    break;

                case Step.Leaving:
                    if (!IsMoving)
                    {
                        SetCurrentStep(Step.Finished);
                        hospital.RegisterPatientDeparture(this, wasTreated);
                    }
                    break;
            }
        }
    }
}
