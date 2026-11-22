using System.Numerics;
using Raylib_cs;

using MiniHospital.Config;

namespace MiniHospital.Rendering
{
    public readonly struct ScreenCanvas
    {
        public float Scale { get; }
        public Vector2 Origin { get; }
        public int HeaderHeight { get; }
        public int Width { get; }
        public int Height { get; }

        public const int ReferenceWidth = 1440;
        public const int ReferenceHeight = 800;


        public ScreenCanvas(int width, int height)
        {
            Width = Math.Max(1, width);
            Height = Math.Max(1, height);
            HeaderHeight = Math.Clamp((int)MathF.Round(width / ReferenceWidth * HudConfig.BaseHeight), HudConfig.MinHeight, HudConfig.MaxHeight);
            Scale = MathF.Min((float)Width / HospitalLayout.Width,
                (float)Math.Max(1, Height - HeaderHeight) / HospitalLayout.Height);
            Origin = new Vector2((Width - HospitalLayout.Width * Scale) / 2f,
                HeaderHeight + (Height - HeaderHeight - HospitalLayout.Height * Scale) / 2f);
        }

        public Vector2 At(Vector2 p)
        {
            return Origin + p * Scale;
        }

        public Rectangle At(float x, float y, float w, float h)
        {
            return new Rectangle(Origin.X + x * Scale, Origin.Y + y * Scale, w * Scale, h * Scale);
        }

        public float LabelSize => Math.Clamp(23f * Scale / .82f, 18f, 27f);
    }

}
