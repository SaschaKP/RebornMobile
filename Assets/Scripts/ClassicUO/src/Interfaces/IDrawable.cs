

using ClassicUO.Renderer;

namespace ClassicUO.Interfaces
{
    internal interface IDrawable
    {
        bool AllowedToDraw { get; set; }

        UOTexture32 Texture { get; set; }

        bool Draw(UltimaBatcher2D batcher, int posX, int posY);
    }

    internal interface IDrawableUI
    {
        UOTexture32 Texture { get; set; }

        bool Draw(UltimaBatcher2D batcher, int x, int y);
    }
}