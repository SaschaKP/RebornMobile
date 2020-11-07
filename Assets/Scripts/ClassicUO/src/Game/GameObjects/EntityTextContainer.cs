

using ClassicUO.Configuration;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility.Collections;

using Microsoft.Xna.Framework;

namespace ClassicUO.Game.GameObjects
{
    class TextContainer : LinkedObject
    {
        public int Size, MaxSize = 5;

        public void Add(TextObject obj)
        {
            PushToBack(obj);

            if (Size >= MaxSize)
            {
                ((TextObject) Items)?.Destroy();
                Remove(Items);
            }
            else
                Size++;
        }


        public new void Clear()
        {
            var item = (TextObject) Items;
            Items = null;

            while (item != null)
            {
                var next = (TextObject) item.Next;
                item.Next = null;
                item.Destroy();
                Remove(item);

                item =  next;
            }

            Size = 0;
        }
    }

    
    internal class OverheadDamage
    {
        private const int DAMAGE_Y_MOVING_TIME = 25;

        private readonly Deque<TextObject> _messages;

        private Rectangle _rectangle;


        public OverheadDamage(GameObject parent)
        {
            Parent = parent;
            _messages = new Deque<TextObject>();
        }


        public GameObject Parent { get; private set; }
        public bool IsDestroyed { get; private set; }
        public bool IsEmpty => _messages.Count == 0;

        public void SetParent(GameObject parent)
        {
            Parent = parent;
        }

        public void Add(int damage)
        {
            TextObject text_obj = TextObject.Create();
            text_obj.RenderedText = RenderedText.Create(damage.ToString(), (ushort) (Parent == World.Player ? 0x0034 : 0x0021), 3, false);
            text_obj.Time = Time.Ticks + 1500;

            _messages.AddToFront(text_obj);

            if (_messages.Count > 10)
                _messages.RemoveFromBack()?.Destroy();
        }

        public void Update()
        {
            if (IsDestroyed)
                return;

            _rectangle.Width = 0;

            for (int i = 0; i < _messages.Count; i++)
            {
                var c = _messages[i];

                float delta = c.Time - Time.Ticks;

                if (c.SecondTime < Time.Ticks)
                {
                    c.OffsetY += 1;
                    c.SecondTime = Time.Ticks + DAMAGE_Y_MOVING_TIME;
                }

                if (delta <= 0)
                {
                    _rectangle.Height -= c.RenderedText?.Height ?? 0;
                    c.Destroy();
                    _messages.RemoveAt(i--);
                }
                //else if (delta < 250)
                //    c.Alpha = 1f - delta / 250;
                else if (c.RenderedText != null)
                {
                    if (_rectangle.Width < c.RenderedText.Width)
                        _rectangle.Width = c.RenderedText.Width;
                }
            }
        }

        public void Draw(UltimaBatcher2D batcher, int x, int y, float scale)
        {
            if (IsDestroyed || _messages.Count == 0)
                return;

            int screenX = ProfileManager.Current.GameWindowPosition.X;
            int screenY = ProfileManager.Current.GameWindowPosition.Y;
            int screenW = ProfileManager.Current.GameWindowSize.X;
            int screenH = ProfileManager.Current.GameWindowSize.Y;

            int offY = 0;


            if (Parent != null)
            {
                x += Parent.RealScreenPosition.X;
                y += Parent.RealScreenPosition.Y;

                _rectangle.X = Parent.RealScreenPosition.X;
                _rectangle.Y = Parent.RealScreenPosition.Y;

                if (Parent is Mobile m)
                {
                    if (m.IsGargoyle && m.IsFlying)
                        offY += 22;
                    else if (!m.IsMounted)
                        offY = -22;


                    AnimationsLoader.Instance.GetAnimationDimensions(m.AnimIndex,
                                                                  m.GetGraphicForAnimation(),
                                                                  /*(byte) m.GetDirectionForAnimation()*/ 0,
                                                                  /*Mobile.GetGroupForAnimation(m, isParent:true)*/ 0,
                                                                  m.IsMounted,
                                                                  /*(byte) m.AnimIndex*/ 0,
                                                                  out int centerX,
                                                                  out int centerY,
                                                                  out int width,
                                                                  out int height);
                    x += (int) m.Offset.X;
                    x += 22;
                    y += (int) (m.Offset.Y - m.Offset.Z - (height + centerY + 8));
                }
                else
                {
                    ArtTexture texture = ArtLoader.Instance.GetTexture(Parent.Graphic);

                    if (texture != null)
                    {
                        x += 22;
                        int yValue = texture.Height >> 1;

                        if (Parent is Item it)
                        {
                            if (it.IsCorpse)
                                offY = -22;
                        }
                        else if (Parent is Static || Parent is Multi)
                            offY = -44;

                        y -= yValue;
                    }

                   
                }
            }


            x = (int) (x / scale);
            y = (int) (y / scale);

            x -= (int) (screenX / scale);
            y -= (int) (screenY / scale);

            x += screenX;
            y += screenY;


            foreach (var item in _messages)
            {
                ushort hue = 0;

                if (item.IsDestroyed || item.RenderedText == null || item.RenderedText.IsDestroyed)
                    continue;

                //if (ProfileManager.Current.HighlightGameObjects)
                //{
                //    if (SelectedObject.LastObject == item)
                //        hue = 23;
                //}
                //else if (SelectedObject.LastObject == item)
                //    hue = 23;

                item.X = x - (item.RenderedText.Width >> 1);
                item.Y = y - offY - item.RenderedText.Height - item.OffsetY;

                item.RenderedText.Draw(batcher, item.X, item.Y, item.Alpha, hue);
                offY += item.RenderedText.Height;
            }
        }


        public void Destroy()
        {
            if (IsDestroyed)
                return;

            IsDestroyed = true;

            foreach (var item in _messages)
                item.Destroy();

            _messages.Clear();
        }
    }
}