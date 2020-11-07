

using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;

namespace ClassicUO.Game.UI.Controls
{
    internal class ClickableColorBox : ColorBox
    {
        private const int CELL = 12;

        private readonly UOTexture32 _background;

        public ClickableColorBox(int x, int y, int w, int h, ushort hue, uint color) : base(w, h, hue, color)
        {
            X = x + 3;
            Y = y + 3;
            WantUpdateSize = false;

            _background = GumpsLoader.Instance.GetTexture(0x00D4);
        }

        public override void Update(double totalMS, double frameMS)
        {
            _background.Ticks = (long) totalMS;

            base.Update(totalMS, frameMS);
        }

        public override bool Draw(UltimaBatcher2D batcher, int x, int y)
        {
            ResetHueVector();
            batcher.Draw2D(_background, x - 3, y - 3, ref _hueVector);

            return base.Draw(batcher, x, y);
        }

        protected override void OnMouseUp(int x, int y, MouseButtonType button)
        {
            if (button == MouseButtonType.Left)
            {
                ColorPickerGump pickerGump = new ColorPickerGump(0, 0, 100, 100, s => SetColor(s, HuesLoader.Instance.GetPolygoneColor(CELL, s)));
                UIManager.Add(pickerGump);
            }
        }
    }
}
