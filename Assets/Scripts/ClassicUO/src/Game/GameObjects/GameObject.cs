

using System;
using System.Runtime.CompilerServices;

using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;

using Microsoft.Xna.Framework;

using IUpdateable = ClassicUO.Interfaces.IUpdateable;

namespace ClassicUO.Game.GameObjects
{
    internal abstract class BaseGameObject : LinkedObject
    {
        public Point RealScreenPosition;
    }

    internal abstract partial class GameObject : BaseGameObject, IUpdateable
    {
        private Point _screenPosition;

        public ushort X, Y;
        public sbyte Z;
        public ushort Hue;
        public ushort Graphic;
        public int CurrentRenderIndex;
        public byte UseInRender;
        public short PriorityZ;
        public GameObject TPrevious;
        public GameObject TNext;
        public Vector3 Offset;
        // FIXME: remove it
        public sbyte FoliageIndex = -1;

        public bool IsDestroyed { get; protected set; }
        public bool IsPositionChanged { get; protected set; }
        public TextContainer TextContainer { get; private set; }
        public int Distance
        {
            [MethodImpl(256)]
            get
            {
                if (World.Player == null /*|| IsDestroyed*/)
                    return ushort.MaxValue;

                if (this == World.Player)
                    return 0;

                int x = X, y = Y;

                if (this is Mobile mobile && mobile.Steps.Count != 0)
                {
                    ref var step = ref mobile.Steps.Back();
                    x = step.X;
                    y = step.Y;
                }

                int fx = World.RangeSize.X;
                int fy = World.RangeSize.Y;

                return Math.Max(Math.Abs(x - fx), Math.Abs(y - fy));
            }
        }

        public virtual void Update(double totalMS, double frameMS)
        {
        }

        [MethodImpl(256)]
        public void AddToTile(int x, int y)
        {
            if (World.Map != null)
            {
                RemoveFromTile();

                if (!IsDestroyed)
                {
                    World.Map.GetChunk(x, y)?.AddGameObject(this, x % 8, y % 8);
                }
            }
        }

        [MethodImpl(256)]
        public void AddToTile()
        {
            AddToTile(X, Y);
        }


        [MethodImpl(256)]
        public void RemoveFromTile()
        {
            if (TPrevious != null)
                TPrevious.TNext = TNext;

            if (TNext != null)
                TNext.TPrevious = TPrevious;

            TNext = null;
            TPrevious = null;
        }

        public virtual void UpdateGraphicBySeason()
        {

        }

        [MethodImpl(256)]
        public void UpdateScreenPosition()
        {
            _screenPosition.X = (X - Y) * 22;
            _screenPosition.Y = (X + Y) * 22 - (Z << 2);
            IsPositionChanged = true;
            OnPositionChanged();
        }

        [MethodImpl(256)]
        public void UpdateRealScreenPosition(int offsetX, int offsetY)
        {
            RealScreenPosition.X = _screenPosition.X - offsetX - 22;
            RealScreenPosition.Y = _screenPosition.Y - offsetY - 22;
            IsPositionChanged = false;

            UpdateTextCoordsV();
        }


        public void AddMessage(MessageType type, string message, TEXT_TYPE text_type)
        {
            AddMessage(type, message, ProfileManager.Current.ChatFont, ProfileManager.Current.SpeechHue, true, text_type);
        }

        public virtual void UpdateTextCoordsV()
        {

        }

        protected void FixTextCoordinatesInScreen()
        {
            if (this is Item it && SerialHelper.IsValid(it.Container))
                return;

            int offsetY = 0;

            int minX = ProfileManager.Current.GameWindowPosition.X + 6;
            int maxX = minX + ProfileManager.Current.GameWindowSize.X;
            int minY = ProfileManager.Current.GameWindowPosition.Y;
            //int maxY = minY + ProfileManager.Current.GameWindowSize.Y - 6;

            for (var item = (TextObject) TextContainer.Items; item != null; item = (TextObject) item.Next)
            {
                if (item.RenderedText == null || item.RenderedText.IsDestroyed || item.RenderedText.Texture == null || item.Time < Time.Ticks)
                    continue;

                int startX = item.RealScreenPosition.X;
                int endX = startX + item.RenderedText.Width;

                if (startX < minX)
                    item.RealScreenPosition.X += minX - startX;

                if (endX > maxX)
                    item.RealScreenPosition.X -= endX - maxX;

                int startY = item.RealScreenPosition.Y;

                if (startY < minY && offsetY == 0)
                    offsetY = minY - startY;

                //int endY = startY + item.RenderedText.Height;

                //if (endY > maxY)
                //    UseInRender = 0xFF;
                //    //item.RealScreenPosition.Y -= endY - maxY;

                if (offsetY != 0)
                    item.RealScreenPosition.Y += offsetY;
            }
        }

        public void AddMessage(MessageType type, string text, byte font, ushort hue, bool isunicode, TEXT_TYPE text_type)
        {
            if (string.IsNullOrEmpty(text))
                return;

            var msg = MessageManager.CreateMessage(text, hue, font, isunicode, type, text_type);
            AddMessage(msg);
        }

        public void AddMessage(TextObject msg)
        {
            if (TextContainer == null)
                TextContainer = new TextContainer();

            msg.Owner = this;
            TextContainer.Add(msg);

            if (this is Item it && SerialHelper.IsValid(it.Container))
            {
                UpdateTextCoordsV();
            }
            else
            {
                IsPositionChanged = true;
                World.WorldTextManager.AddMessage(msg);
            }
        }


        protected virtual void OnPositionChanged()
        {
        }

        protected virtual void OnDirectionChanged()
        {
        }

        public virtual void Destroy()
        {
            if (IsDestroyed)
                return;

            Next = null;
            Previous = null;

            Clear();
            RemoveFromTile();
            TextContainer?.Clear();

            IsDestroyed = true;
            PriorityZ = 0;
            IsPositionChanged = false;
            Hue = 0;
            Offset = Vector3.Zero;
            CurrentRenderIndex = 0;
            UseInRender = 0;
            RealScreenPosition = Point.Zero;
            _screenPosition = Point.Zero;
            IsFlipped = false;
            Graphic = 0;
            UseObjectHandles = ClosedObjectHandles = ObjectHandlesOpened = false;
            FrameInfo = Rectangle.Empty;
        }
    }
}