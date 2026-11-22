using System.Numerics;

namespace MiniHospital.Config
{
    public class Slot
    {
        public Vector2 Anchor { get; }
        public Vector2 Approach { get; }
        public NavNode AccessNode { get; }

        public CharacterAction Pose {  get; }

        public Facing Facing { get; }
        public float Size;

        public Slot(Vector2 approach, Vector2 anchor, NavNode accessNode, CharacterAction pose,  Facing facing = Facing.Right, float size = 1f)
        {
            Approach = approach;
            Anchor = anchor;
            AccessNode = accessNode;
            Pose = pose;
            Facing = facing;
            Size = size;
        }
    }

}