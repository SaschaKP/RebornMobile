

using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Renderer
{
    internal static class Texture2DCache
    {
        private static readonly Dictionary<Color, Texture2D> _textures = new Dictionary<Color, Texture2D>();

        public static Texture2D GetTexture(Color color)
        {
            if (!_textures.TryGetValue(color, out var t))
            {
                t = new Texture2D(Client.Game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                t.SetData(new[] {color});
                _textures[color] = t;
            }

            return t;
        }

        public static void Dispose()
        {
            foreach (var keyValuePair in _textures)
            {
                keyValuePair.Value?.Dispose();
            }
            _textures.Clear();
        }
    }
}