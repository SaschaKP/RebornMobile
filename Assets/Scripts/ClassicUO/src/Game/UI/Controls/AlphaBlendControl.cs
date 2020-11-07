

using ClassicUO.Renderer;

using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Controls
{
    internal sealed class AlphaBlendControl : Control
    {
        public AlphaBlendControl(float alpha = 0.5f)
        {
            Alpha = alpha;
            AcceptMouseInput = false;
        }

        public ushort Hue { get; set; }

        public override bool Draw(UltimaBatcher2D batcher, int x, int y)
        {
            ResetHueVector();
            
            ShaderHueTranslator.GetHueVector(ref _hueVector, Hue, false, Alpha);

            return batcher.Draw2D(Texture2DCache.GetTexture(Color.Black), x, y, Width, Height, ref _hueVector);
        }
    }
}