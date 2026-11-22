namespace MiniHospital.Utils
{
    public class TimeRange
    {
        public float Min { get; }
        public float Max { get; }

        public TimeRange(float min, float max)
        {
            if (min < 0 || max < min)
                throw new ArgumentOutOfRangeException(nameof(max));

            Min = min;
            Max = max;
        }

        public float Sample() => Min + Random.Shared.NextSingle() * (Max - Min);
    }
}
