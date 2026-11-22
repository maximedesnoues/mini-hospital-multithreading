using System.Threading;

namespace MiniHospital.Simulation
{
    public class HospitalStats
    {
        private readonly object statsLock = new();

        private int arrived;
        private int treated;
        private int untreated;
        private int active;

        private int score;
        private int combo;
        private int bestCombo;

        public int Arrived => Volatile.Read(ref arrived);
        public int Treated => Volatile.Read(ref treated);
        public int Untreated => Volatile.Read(ref untreated);
        public int Active => Volatile.Read(ref active);

        public int Score
        {
            get
            {
                lock (statsLock)
                {
                    return score;
                }
            }
        }

        public int Combo
        {
            get
            {
                lock (statsLock)
                {
                    return combo;
                }
            }
        }

        public int BestCombo
        {
            get
            {
                lock (statsLock)
                {
                    return bestCombo;
                }
            }
        }

        public void RegisterArrival()
        {
            lock (statsLock)
            {
                Interlocked.Increment(ref arrived);
                Interlocked.Increment(ref active);
            }
        }

        public void RegisterDeparture(bool wasTreated)
        {
            lock (statsLock)
            {
                if (active <= 0)
                {
                    throw new InvalidOperationException("Départ enregistré alors qu'aucun patient n'est actif");
                }

                Interlocked.Decrement(ref active);

                if (wasTreated)
                {
                    Interlocked.Increment(ref treated);
                }
                else
                {
                    Interlocked.Increment(ref untreated);
                }

                UpdateScoreCore(wasTreated);
            }
        }

        public void UpdateScore(bool treated)
        {
            lock (statsLock)
            {
                UpdateScoreCore(treated);
            }
        }

        private void UpdateScoreCore(bool treated)
        {
            if (treated)
            {
                combo++;
                score += 100 * combo;

                if (combo > bestCombo)
                {
                    bestCombo = combo;
                }
            }
            else
            {
                combo = 0;
            }
        }
    }
}
