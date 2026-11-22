using Raylib_cs;
using System.Numerics;

namespace MiniHospital.Config
{

    public class SpriteConfig
    {
        public TextureId Texture { get; }
        public Rectangle Source { get; }
        public Vector2 Position { get; }
        public Vector2 Anchor { get; }
        public int Layer { get; }
        public float Scale { get; }

        public SpriteConfig(TextureId texture, Rectangle source,
            Vector2 position, Vector2 anchor, int layer = 0, float scale = 1f)
        {
            Texture = texture;
            Source = source;
            Position = position;
            Anchor = anchor;
            Layer = layer;
            Scale = scale;
        }
    }
}
