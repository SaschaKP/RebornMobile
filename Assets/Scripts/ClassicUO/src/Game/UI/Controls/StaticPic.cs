

using System.Collections.Generic;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility;

namespace ClassicUO.Game.UI.Controls
{
    internal class StaticPic : Control
    {
        public StaticPic(ushort graphic, ushort hue)
        {
            Hue = hue;
            IsPartialHue = TileDataLoader.Instance.StaticData[graphic].IsPartialHue;
            CanMove = true;

            var texture = ArtLoader.Instance.GetTexture(graphic);

            if (texture == null)
            {
                Dispose();
                return;
            }

            Width = texture.Width;
            Height = texture.Height;
            Graphic = graphic;
            WantUpdateSize = false;
        }

        public StaticPic(List<string> parts) : this(UInt16Converter.Parse(parts[3]), parts.Count > 4 ? UInt16Converter.Parse(parts[4]) : (ushort) 0)
        {
            X = int.Parse(parts[1]);
            Y = int.Parse(parts[2]);
            IsFromServer = true;
        }


        public ushort Hue { get; set; }
        public bool IsPartialHue { get; set; }
        public ushort Graphic { get; }


        public override bool Draw(UltimaBatcher2D batcher, int x, int y)
        {
            ResetHueVector();
            ShaderHueTranslator.GetHueVector(ref _hueVector, Hue, IsPartialHue, 0);

            var texture = ArtLoader.Instance.GetTexture(Graphic);

            if (texture != null)
            {
                batcher.Draw2D(texture, x, y, Width, Height, ref _hueVector);
            }

            return base.Draw(batcher, x, y);
        }

        public override bool Contains(int x, int y)
        {
            var texture = ArtLoader.Instance.GetTexture(Graphic);

            return texture != null && texture.Contains(x, y);
        }
    }
}