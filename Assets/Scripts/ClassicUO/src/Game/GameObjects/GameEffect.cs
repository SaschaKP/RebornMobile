

using System.Collections.Generic;

using ClassicUO.Game.Data;
using ClassicUO.IO.Resources;

namespace ClassicUO.Game.GameObjects
{
    internal abstract class GameEffect : GameObject
    {
        public AnimDataFrame2 AnimDataFrame;

        protected GameEffect()
        {
            Children = new List<GameEffect>();
            AlphaHue = 0xFF;
        }

        public List<GameEffect> Children { get; }

        public GameObject Source;

        protected GameObject Target;

        protected int TargetX;

        protected int TargetY;

        protected int TargetZ;

        public int IntervalInMs;

        public long NextChangeFrameTime;

        public bool IsEnabled;

        public ushort AnimationGraphic = 0xFFFF;

        public bool IsMoving => Target != null || TargetX != 0 && TargetY != 0;

        public GraphicEffectBlendMode Blend;

        public long Duration = -1;
        public byte AnimIndex;

        public void Load()
        {
            AnimDataFrame = AnimDataLoader.Instance.CalculateCurrentGraphic(Graphic);
            IsEnabled = true;
            AnimIndex = 0;

            if (AnimDataFrame.FrameInterval == 0)
            {
                IntervalInMs = Constants.ITEM_EFFECT_ANIMATION_DELAY;
            }
            else
            {
                IntervalInMs = AnimDataFrame.FrameInterval * Constants.ITEM_EFFECT_ANIMATION_DELAY;
            }
        }

        public override void Update(double totalMS, double frameMS)
        {
            base.Update(totalMS, frameMS);


            if (Source != null && Source.IsDestroyed)
            {
                World.RemoveEffect(this);

                return;
            }

            if (IsDestroyed)
                return;

            if (IsEnabled)
            {
                if (Duration < totalMS && Duration >= 0)
                    World.RemoveEffect(this);
                //else
                //{
                //    unsafe
                //    {
                //        int count = AnimDataFrame.FrameCount;
                //        if (count == 0)
                //            count = 1;

                //        AnimationGraphic = (Graphic) (Graphic + AnimDataFrame.FrameData[((int) Math.Max(1, (_start / 50d) / Speed)) % count]);
                //    }

                //    _start += frameMS;
                //}

                else if (NextChangeFrameTime < totalMS)
                {

                    if (AnimDataFrame.FrameCount != 0)
                    {
                        unsafe
                        {
                            AnimationGraphic = (ushort) (Graphic + AnimDataFrame.FrameData[AnimIndex]);
                        }

                        AnimIndex++;

                        if (AnimIndex >= AnimDataFrame.FrameCount)
                            AnimIndex = 0;
                    }
                    else
                    {
                        if (Graphic != AnimationGraphic)
                            AnimationGraphic = Graphic;
                    }

                    NextChangeFrameTime = (long) totalMS + IntervalInMs;
                }
            }
            else if (Graphic != AnimationGraphic)
                AnimationGraphic = Graphic;
        }

        public void AddChildEffect(GameEffect effect)
        {
            Children.Add(effect);
        }

        protected (int x, int y, int z) GetSource()
        {
            return Source == null ? (X, Y, Z) : (Source.X, Source.Y, Source.Z);
        }

        public void SetSource(GameObject source)
        {
            Source = source;
            X = source.X;
            Y = source.Y;
            Z = source.Z;
            UpdateScreenPosition();
            AddToTile();
        }

        public void SetSource(int x, int y, int z)
        {
            Source = null;
            X = (ushort) x;
            Y = (ushort) y;
            Z = (sbyte) z;
            UpdateScreenPosition();
            AddToTile();
        }

        protected (int x, int y, int z) GetTarget()
        {
            return Target == null ? (TargetX, TargetY, TargetZ) : (Target.X, Target.Y, Target.Z);
        }

        public void SetTarget(GameObject target)
        {
            Target = target;
        }

        public void SetTarget(int x, int y, int z)
        {
            Target = null;
            TargetX = x;
            TargetY = y;
            TargetZ = z;
        }

        public override void Destroy()
        {
            AnimIndex = 0;
            Source = null;
            Target = null;
            base.Destroy();
        }
    }
}