using MiniHospital.Config;
using Raylib_cs;
using System.Numerics;

namespace MiniHospital.Rendering
{

    public class Sprite
    {
        public Texture2D Texture { get; set; }
        public Rectangle Source { get; set; }

        public Vector2 Position { get; set; }
        public Vector2 Anchor { get; set; }

        public float Scale { get; set; } = 1f;
        public Facing Facing { get; set; } = Facing.Right;
        public int Layer { get; set; } = 0;

        public Sprite(Texture2D texture, Rectangle source,
                      Vector2 position, Vector2 anchor)
        {
            Texture = texture;
            Source = source;
            Position = position;
            Anchor = anchor;
        }

        protected Sprite(Vector2 position, Vector2 anchor) : this(default, default, position, anchor)
        {
        }

        public Sprite Copy() 
        {
        	return (Sprite)MemberwiseClone();
        }
        
        public virtual void Draw(ScreenCanvas canvas)
        {
            Rectangle source = Source;

            float width = MathF.Abs(source.Width);
            float height = MathF.Abs(source.Height);

            Vector2 origin = Anchor * Scale * canvas.Scale;

            if (Facing == Facing.Left)
            {
                source.Width = -width;
                origin.X = (width - Anchor.X) * Scale * canvas.Scale;
            }

            Rectangle destination = canvas.At(
                Position.X,
                Position.Y,
                width * Scale,
                height * Scale
            );

            Raylib.DrawTexturePro(
                Texture,
                source,
                destination,
                origin,
                0f,
                Color.White
            );

            if (HospitalRenderer.DrawDebug)
            {
                Raylib.DrawRectangleRec(
                    canvas.At(Position.X - 2, Position.Y - 2, 4, 4),
                    Color.Red
                );

            }
        }

    }
}