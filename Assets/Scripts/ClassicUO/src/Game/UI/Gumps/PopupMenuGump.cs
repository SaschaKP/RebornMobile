

using ClassicUO.Game.Data;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Utility;

using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps
{
    internal class PopupMenuGump : Gump
    {
        public PopupMenuGump(PopupMenuData data) : base(0, 0)
        {
            CanMove = false;
            CanCloseWithRightClick = true;


            ResizePic pic = new ResizePic(0x0A3C)
            {
                Alpha = 0.25f
            };
            Add(pic);
            int offsetY = 10;
            bool arrowAdded = false;
            int width = 0, height = 20;

            for (int i = 0; i < data.Items.Length; i++)
            {
                ref var item = ref data.Items[i];

                string text = ClilocLoader.Instance.GetString(item.Cliloc);

                ushort hue = item.Hue;

                if (item.ReplacedHue != 0)
                {
                    uint h = HuesHelper.Color16To32(item.ReplacedHue);
                    (byte b, byte g, byte r, byte a) = HuesHelper.GetBGRA(h);

                    Color c = new Color(r, g, b, a);

                    if (c.A == 0)
                        c.A = 0xFF;

                    FontsLoader.Instance.SetUseHTML(true, HuesHelper.RgbaToArgb(c.PackedValue));
                }

                Label label = new Label(text, true, hue, font: 1)
                {
                    X = 10,
                    Y = offsetY
                };
                FontsLoader.Instance.SetUseHTML(false);

                HitBox box = new HitBox(10, offsetY, label.Width, label.Height)
                {
                    Tag = item.Index
                };

                box.MouseUp += (sender, e) =>
                {
                    if (e.Button == MouseButtonType.Left)
                    {
                        HitBox l = (HitBox) sender;
                        GameActions.ResponsePopupMenu(data.Serial, (ushort) l.Tag);
                        Dispose();
                    }
                };
                Add(box);
                Add(label);

                if ((item.Flags & 0x02) != 0 && !arrowAdded)
                {
                    arrowAdded = true;

                    // TODO: wat?
                    Add(new Button(0, 0x15E6, 0x15E2, 0x15E2)
                    {
                        X = 20,
                        Y = offsetY
                    });
                    height += 20;
                }

                offsetY += label.Height;

                if (!arrowAdded)
                {
                    height += label.Height;

                    if (width < label.Width)
                        width = label.Width;
                }
            }

            width += 20;

            if (height <= 10 || width <= 20)
                Dispose();
            else
            {
                pic.Width = width;
                pic.Height = height;
                foreach (HitBox box in FindControls<HitBox>())
                    box.Width = width - 20;
            }
        }


    }
}