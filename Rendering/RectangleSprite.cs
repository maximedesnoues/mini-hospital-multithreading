using Raylib_cs;
using System.Numerics;

namespace MiniHospital.Rendering
{
    public class RectangleSprite : Sprite
    {
        private Rectangle area;
        private Color color;

        public RectangleSprite(Rectangle area, Color color) :
            base(new Vector2(area.X + area.Width / 2f, area.Y + area.Height), new Vector2(area.Width / 2f, area.Height))
        {
            this.area = area;
            Layer = 2;
            this.color = color;
        }

        public override void Draw(ScreenCanvas canvas)
        {
            Raylib.DrawRectangleRec(
                canvas.At(area.X, area.Y, area.Width, area.Height),
                color
            );
        }
    }
}
