using MiniHospital.Config;
using MiniHospital.Rendering;
using MiniHospital.Simulation.Rooms;
using MiniHospital.Utils;
using System.Numerics;
using System.Threading;

namespace MiniHospital.Simulation.Characters
{
    public abstract class Character
    {
        protected readonly object characterLock = new();

        public CharacterAction Action { get; private set; }
        public bool IsMoving
        {
            get
            {
                lock (characterLock)
                {
                    return path != null;
                }
            }
        }



        private int characterSpriteIndex;
        private AnimatedSprite sprite;

        protected Vector2 position;

        private AtlasSprite bubbleSprite;
        private BubbleType bubbleType;

        private Vector2[]? path;
        private int pathIndex;
        private Slot? arrivalSlot;
        private NavNode? arrivalNode;

        private Slot? currentSlot;
        private NavNode currentNode;

        private float moveSpeed;

        public Character(int index) 
        {
            characterSpriteIndex = index;
            Action = CharacterAction.Standing;
            position = new Vector2();
            sprite = new AnimatedSprite(
                TextureManager.Get(TextureId.Characters), 
                CharacterConfig.GetFrames(characterSpriteIndex, Action),
                position,
                CharacterConfig.DefaultAnchor);
            bubbleSprite = new AtlasSprite(TextureManager.Get(TextureId.Bubbles), 96, 96, new Vector2(0, 0), new Vector2(48, 96));
            sprite.Scale = CharacterConfig.RenderScale;
            bubbleSprite.Scale = CharacterConfig.BubbleScale;
            bubbleSprite.SetIndex(0);
            bubbleSprite.Layer = 2;

            moveSpeed = 120f;
        }

        protected void SetRenderScale(float scale)
        {
            sprite.Scale = scale;
        }

        public void SetCharacterAction(CharacterAction action)
        {
            if (Action == action)
                return;

            Action = action;
            sprite.SetFrames(CharacterConfig.GetFrames(characterSpriteIndex, action));
        }


        public virtual void Update(float deltaTime)
        {
            lock (characterLock)
            {
                if (Hospital.CallBrainOnUpdate && Hospital.CurrentQuestion == 0)
                {
                    Brain();
                }

                if (path != null)
                {
                    if (UpdatMoveTo(deltaTime, path[pathIndex]))
                    {
                        pathIndex++;

                        if (pathIndex >= path.Length)
                        {
                            path = null;
                            Chaos.ExecuteIfEnabled(2);

                            if (arrivalSlot != null)
                                PlaceAt(arrivalSlot);
                            else if (arrivalNode.HasValue)
                                PlaceAt(arrivalNode.Value);
                            else
                                SetCharacterAction(CharacterAction.Standing);

                            arrivalNode = null;
                            arrivalSlot = null;
                        }
                    }
                }

                if (sprite != null)
                {
                    sprite.Update(deltaTime);
                    sprite.Position = position;
                }

                if (bubbleSprite != null)
                {
                    bubbleSprite.Position = position + CharacterConfig.BubblePosition;
                }
            }
        }

        private bool UpdatMoveTo(float deltaTime, Vector2 target)
        {
            Vector2 move = target - position;
            float dist = moveSpeed * deltaTime;
            if (move.LengthSquared() <= dist * dist)
            {
                position = target;
                return true;
            }

            position += move * dist / move.Length();
            sprite.Facing = move.X < 0f ? Facing.Left : Facing.Right;
            return false;
        }

        public void PlaceAt(NavNode node)
        {
            currentNode = node;
            currentSlot = null;

            position = HospitalLayout.NavPoints[node];
            sprite.Position = position;
            SetCharacterAction(CharacterAction.Standing);
        }

        public void PlaceAt(Slot slot)
        {
            currentSlot = slot;
            currentNode = slot.AccessNode;

            position = slot.Anchor;
            sprite.Position = slot.Anchor;
            sprite.Facing = slot.Facing;
            SetCharacterAction(slot.Pose);
        }

        private void FollowPath(Vector2[] points, Slot? destination = null)
        {
            path = points;
            pathIndex = 0;
            arrivalSlot = destination;

            if (points.Length > 0)
                SetCharacterAction(CharacterAction.Walking);
        }

        public void MoveTo(Slot destination)
        {
            List<Vector2> points = new List<Vector2>();

            // Quitter l'emplacement actuel.
            if (currentSlot != null)
                points.Add(currentSlot.Approach);

            // Traverser l'hôpital.
            points.AddRange(Navigation.FindPath(
                currentNode,
                destination.AccessNode
            ));

            // Rejoindre le point d'approche final.
            points.Add(destination.Approach);

            FollowPath(points.ToArray(), destination);
        }

        public void MoveTo(Room room)
        {
            MoveTo(room.Config.Patient);
        }
        public void MoveTo(NavNode destination)
        {
            List<Vector2> points = new List<Vector2>();

            if (currentSlot != null)
                points.Add(currentSlot.Approach);

            points.AddRange(Navigation.FindPath(currentNode, destination));

            FollowPath(points.ToArray());
            arrivalNode = destination;
        }

        public virtual void GetRenderSprites(List<Sprite> list)
        {
            lock (characterLock)
            {
                list.Add(sprite.Copy());

                if (bubbleType != BubbleType.None)
                    list.Add(bubbleSprite.Copy());
            }
        }

        protected void SetBubbletype(BubbleType type)
        {
            bubbleType = type;
            if (type != BubbleType.None)
                bubbleSprite.SetIndex((int)type - 1);
        }

        protected virtual bool IsBrainFinished => false;

        protected virtual void AfterBrainIteration()
        {
        }

        public Thread StartBrainThread()
        {
            Thread brainThread = new Thread(() =>
            {
                while (true)
                {
                    lock (characterLock)
                    {
                        if (IsBrainFinished)
                        {
                            break;
                        }

                        Brain();
                    }

                    AfterBrainIteration();

                    Thread.Sleep(10);
                }
            });

            brainThread.IsBackground = true;
            brainThread.Start();

            return brainThread;
        }

        protected abstract void Brain();
    }
}
