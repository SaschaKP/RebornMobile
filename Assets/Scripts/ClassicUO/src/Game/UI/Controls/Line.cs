

using System;

using ClassicUO.Game.UI.Gumps;
using ClassicUO.Renderer;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Controls
{
    internal class Line : Control
    {
        private readonly Texture2D _texture;

        public Line(int x, int y, int w, int h, uint color)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
            _texture = Texture2DCache.GetTexture(new Color() { PackedValue = color });
        }

        internal static Line[] CreateRectangleArea(Gump g, int startx, int starty, int width, int height, int topage = 0, uint linecolor = 0xAFAFAF, int linewidth = 1, string toplabel = null, ushort textcolor = 999, byte textfont = 0xFF)
        {
            Line[] lines = new Line[3];
            if (!string.IsNullOrEmpty(toplabel))
            {
                Label l = new Label(toplabel, true, textcolor, font: textfont);
                int rwidth = (width - l.Width) >> 1;
                l.X = startx + rwidth + 2;
                l.Y = Math.Max(0, starty - ((l.Height + 1) >> 1));
                g.Add(l, topage);

                if (rwidth > 0)
                {
                    g.Add(new Line(startx, starty, rwidth, linewidth, linecolor), topage);
                    g.Add(new Line(startx + width - rwidth, starty, rwidth, linewidth, linecolor), topage);
                }
            }
            else
                g.Add(new Line(startx, starty, width, linewidth, linecolor), topage);

            g.Add(lines[0] = new Line(startx, starty, linewidth, height, linecolor), topage);
            g.Add(lines[1] = new Line(startx + width - 1, starty, linewidth, height, linecolor), topage);
            g.Add(lines[2] = new Line(startx, starty + height - 1, width, linewidth, linecolor), topage);

            return lines;
        }

        public override void Update(double totalMS, double frameMS)
        {
            base.Update(totalMS, frameMS);
        }

        public override bool Draw(UltimaBatcher2D batcher, int x, int y)
        {
            ResetHueVector();
            ShaderHueTranslator.GetHueVector(ref _hueVector, 0, false, Alpha);

            return batcher.Draw2D(_texture, x, y, Width, Height, ref _hueVector);
        }
    }
}