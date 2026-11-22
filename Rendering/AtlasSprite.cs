using Raylib_cs;
using System.Numerics;

namespace MiniHospital.Rendering
{
    public class AtlasSprite : Sprite
    {
        public int FrameWidth { get; }
        public int FrameHeight { get; }

        public AtlasSprite(
            Texture2D texture,
            int frameWidth,
            int frameHeight,
            Vector2 position,
            Vector2 anchor)
            : base(
                texture,
                new Rectangle(0, 0, frameWidth, frameHeight),
                position,
                anchor)
        {
            FrameWidth = frameWidth;
            FrameHeight = frameHeight;
        }

        public void SetIndex(int index)
        {
            int columns = Texture.Width / FrameWidth;

            Source = new Rectangle(
                (index % columns) * FrameWidth,
                (index / columns) * FrameHeight,
                FrameWidth,
                FrameHeight
            );
        }
    }
}
