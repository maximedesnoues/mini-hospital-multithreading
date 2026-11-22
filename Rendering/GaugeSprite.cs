using Raylib_cs;
using System.Numerics;

namespace MiniHospital.Rendering
{
    public class GaugeSprite : Sprite
    {
        public float Value { get; set; } = 1f;

        public float Width { get; set; } = MiniHospital.Config.CharacterConfig.GaugeWidth;
        public float Height { get; set; } = MiniHospital.Config.CharacterConfig.GaugeHeight;

        public GaugeSprite()
            : base(Vector2.Zero, Vector2.Zero)
        {
            Layer = 2;
        }

        public override void Draw(ScreenCanvas canvas)
        {
            float ratio = Math.Clamp(Value, 0f, 1f);

            Rectangle background = canvas.At(
                Position.X - Width / 2,
                Position.Y,
                Width,
                Height
            );

            Rectangle fill = canvas.At(
                Position.X - Width / 2,
                Position.Y,
                Width * ratio,
                Height
            );

            Color color = ratio < 0.25f
                ? Color.Red
                : ratio < 0.5f
                    ? Color.Orange
                    : Color.Green;

            Raylib.DrawRectangleRec(
                background,
                new Color(25, 35, 50, 220)
            );

            Raylib.DrawRectangleRec(fill, color);
        }
    }
}
