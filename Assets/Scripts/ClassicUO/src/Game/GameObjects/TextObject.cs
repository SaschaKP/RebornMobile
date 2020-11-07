

using System.Collections.Generic;

using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Renderer;
using ClassicUO.Utility;

using Microsoft.Xna.Framework;

namespace ClassicUO.Game.GameObjects
{
    internal class TextObject : BaseGameObject
    {
        private static readonly QueuedPool<TextObject> _queue = new QueuedPool<TextObject>(1000, o =>
        {
            o.IsDestroyed = false;
            o.Alpha = 0;
            o.Hue = 0;
            o.Time = 0;
            o.IsTransparent = false;
            o.SecondTime = 0;
            o.Type = 0;
            o.X = 0;
            o.Y = 0;
            o.RealScreenPosition = Point.Zero;
            o.OffsetY = 0;
            o.Owner = null;
            o.UnlinkD();
            o.IsTextGump = false;
            o.RenderedText?.Destroy();
            o.RenderedText = null;
            o.Clear();
        });


        public static TextObject Create()
        {
            return _queue.GetOne();
        }

        public byte Alpha;
        public ushort Hue;
        public bool IsTransparent;

        public RenderedText RenderedText;
        public long Time, SecondTime;
        public MessageType Type;
        public int X, Y, OffsetY;
        public GameObject Owner;
        public TextObject DLeft, DRight;
        public bool IsDestroyed;
        public bool IsTextGump;


        public virtual void Destroy()
        {
            if (IsDestroyed)
                return;

            UnlinkD();

            RealScreenPosition = Point.Zero;
            IsDestroyed = true;
            RenderedText?.Destroy();
            RenderedText = null;
            Owner = null;

            _queue.ReturnOne(this);
        }

        public void UnlinkD()
        {
            if (DRight != null)
                DRight.DLeft = DLeft;

            if (DLeft != null)
                DLeft.DRight = DRight;

            DRight = null;
            DLeft = null;
        }

        public void ToTopD()
        {
            var obj = this;

            while (obj != null)
            {
                if (obj.DLeft == null)
                    break;

                obj = obj.DLeft;
            }

            var next = (TextRenderer) obj;
            next.MoveToTop(this);
        }
    }
}
