using Raylib_cs;
using System.Numerics;

namespace MiniHospital.Config;

public static class CharacterConfig
{
    public const int FrameWidth = 120;
    public const int FrameHeight = 140;
    public const float RenderScale = 0.64f;
    public const float PatientScale = 0.64f;
    public const float DoctorScale = 0.64f;
    public const float BubbleScale = 0.44f;

    public const float GaugeWidth = 39f;
    public const float GaugeHeight = 5f;

    public static Vector2 DefaultAnchor { get; } = new Vector2(FrameWidth / 2, 136);

    public static int[] PatientSpriteIds = { 0, 1, 2, 3, 4, 5 };
    public static int[] DoctorSpriteIds = { 6, 7, 8 };
    public static int[] ReceptionitsSpriteIds = { 9, 10, 11 };

    public static Vector2 BubblePosition { get; } = new Vector2(2f, -91f);
    public static Vector2 PatienceGaugePosition { get; } = new Vector2(0f, -87f);

    public static readonly Dictionary<CharacterAction, int[]> Animations = new()
    {
        { CharacterAction.Standing, [0] },
        { CharacterAction.Walking,  [1, 2] },
        { CharacterAction.Sitting,  [3] },
        { CharacterAction.Lying,   [4] }
    };

    public static Rectangle[] GetFrames(int characterIndex, CharacterAction action)
    {
        int[] columns = Animations[action];
        Rectangle[] frames = new Rectangle[columns.Length];

        for (int i = 0; i < columns.Length; i++)
        {
            frames[i] = new Rectangle(
                columns[i] * FrameWidth,
                characterIndex * FrameHeight,
                FrameWidth,
                FrameHeight
            );
        }

        return frames;
    }
}