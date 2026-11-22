using Raylib_cs;
using System.Numerics;

namespace MiniHospital.Rendering
{
    public class AnimatedSprite : Sprite
    {
        private Rectangle[] frames;
        private int frameIndex = 0;
        private float elapsed = 0f;

        public float FrameDuration { get; set; } = 0.15f;

        public AnimatedSprite(
            Texture2D texture,
            Rectangle[] frames,
            Vector2 position,
            Vector2 anchor)
            : base(texture, frames[0], position, anchor)
        {
            this.frames = frames;
        }

        public void SetFrames(Rectangle[] frames)
        {
            if (ReferenceEquals(this.frames, frames))
                return;

            this.frames = frames;
            frameIndex = 0;
            elapsed = 0f;
            Source = frames[0];
        }

        public void Update(float deltaTime)
        {
            if (frames.Length <= 1)
                return;

            elapsed += deltaTime;

            while (elapsed >= FrameDuration)
            {
                elapsed -= FrameDuration;
                frameIndex = (frameIndex + 1) % frames.Length;
                Source = frames[frameIndex];
            }
        }
    }
}