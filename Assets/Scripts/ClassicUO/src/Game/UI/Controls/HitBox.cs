

using ClassicUO.Renderer;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Controls
{
    internal class HitBox : Control
    {
        protected readonly Texture2D _texture;

        public HitBox(int x, int y, int w, int h, string tooltip = null, float alpha = 0.75f)
        {
            CanMove = false;
            AcceptMouseInput = true;
            Alpha = alpha;
            _texture = Texture2DCache.GetTexture(Color.White);

            X = x;
            Y = y;
            Width = w;
            Height = h;
            WantUpdateSize = false;

            SetTooltip(tooltip);
        }


        public override ClickPriority Priority { get; set; } = ClickPriority.High;


        public override bool Draw(UltimaBatcher2D batcher, int x, int y)
        {
            if (IsDisposed)
                return false;

            if (MouseIsOver)
            {
                ResetHueVector();
                ShaderHueTranslator.GetHueVector(ref _hueVector, 0, false, Alpha, true);

                batcher.Draw2D(_texture, x, y, 0, 0, Width, Height, ref _hueVector);
            }

            return base.Draw(batcher, x, y);
        }
    }
}