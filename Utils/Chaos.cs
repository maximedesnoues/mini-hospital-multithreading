using MiniHospital.Simulation;

namespace MiniHospital.Utils
{
    // Outil pour aider à produire et tester des situations problématiques en Multithreading :
    // ajoute de courtes pauses aléatoires à quelques endroits sensibles pour rendre visibles les problèmes de concurrence.
    public static class Chaos
    {
        public static bool Enabled { get; set; }

        public static void ExecuteIfEnabled(int maxMs = 15)
        {
            if (!Enabled || Hospital.CurrentQuestion == 0)
                return;
            if (Random.Shared.Next(3) != 0)
                return;
            Thread.Sleep(Random.Shared.Next(1, maxMs + 1));
        }
    }
}
