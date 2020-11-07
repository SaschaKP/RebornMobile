

using ClassicUO.IO.Resources;
using ClassicUO.Renderer;

namespace ClassicUO.Game.UI.Controls
{
    internal class Panel : Control
    {
        private readonly UOTexture32[] _frame = new UOTexture32[9];

        public Panel(ushort background)
        {
            for (int i = 0; i < _frame.Length; i++)
                _frame[i] = GumpsLoader.Instance.GetTexture((ushort) (background + i));
        }

        public override void Update(double totalMS, double frameMS)
        {
            foreach (UOTexture32 t in _frame)
            {
                if (t != null)
                    t.Ticks = (long) totalMS;
            }

            base.Update(totalMS, frameMS);
        }

        public override bool Draw(UltimaBatcher2D batcher, int x, int y)
        {
            ResetHueVector();

            int centerWidth = Width - _frame[0].Width - _frame[2].Width;
            int centerHeight = Height - _frame[0].Height - _frame[6].Height;
            int line2Y = y + _frame[0].Height;
            int line3Y = y + Height - _frame[6].Height;
            // top row
            batcher.Draw2D(_frame[0], x, y, ref _hueVector);
            batcher.Draw2DTiled(_frame[1], x + _frame[0].Width, y, centerWidth, _frame[0].Height, ref _hueVector);
            batcher.Draw2D(_frame[2], x + Width - _frame[2].Width, y, ref _hueVector);
            // middle
            batcher.Draw2DTiled(_frame[3], x, line2Y, _frame[3].Width, centerHeight, ref _hueVector);
            batcher.Draw2DTiled(_frame[4], x + _frame[3].Width, line2Y, centerWidth, centerHeight, ref _hueVector);
            batcher.Draw2DTiled(_frame[5], x + Width - _frame[5].Width, line2Y, _frame[5].Width, centerHeight, ref _hueVector);
            // bottom
            batcher.Draw2D(_frame[6], x, line3Y, ref _hueVector);
            batcher.Draw2DTiled(_frame[7], x + _frame[6].Width, line3Y, centerWidth, _frame[6].Height, ref _hueVector);
            batcher.Draw2D(_frame[8], x + Width - _frame[8].Width, line3Y, ref _hueVector);

            return base.Draw(batcher, x, y);
        }
    }
}