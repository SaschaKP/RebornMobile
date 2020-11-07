

using System.Collections.Generic;

using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;

namespace ClassicUO.Game.GameObjects
{
    internal sealed partial class Static : GameObject
    {
        private static readonly QueuedPool<Static> _pool = new QueuedPool<Static>(Constants.PREDICTABLE_STATICS, s =>
        {
            s.IsDestroyed = false;
            s.AlphaHue = 0;
            s.FoliageIndex = 0;
        });


        public static Static Create(ushort graphic, ushort hue, int index)
        {
            Static s = _pool.GetOne();
            s.Graphic = s.OriginalGraphic = graphic;
            s.Hue = hue;
            s.UpdateGraphicBySeason();

            if (s.ItemData.Height > 5)
                s._canBeTransparent = 1;
            else if (s.ItemData.IsRoof || s.ItemData.IsSurface && s.ItemData.IsBackground || s.ItemData.IsWall)
                s._canBeTransparent = 1;
            else if (s.ItemData.Height == 5 && s.ItemData.IsSurface && !s.ItemData.IsBackground)
                s._canBeTransparent = 1;
            else
                s._canBeTransparent = 0;

            return s;
        }

        public string Name => ItemData.Name;

        public ushort OriginalGraphic { get; private set; }

        public bool IsVegetation;

        public ref StaticTiles ItemData => ref TileDataLoader.Instance.StaticData[Graphic];

        public void SetGraphic(ushort g)
        {
            Graphic = g;
        }

        public void RestoreOriginalGraphic()
        {
            Graphic = OriginalGraphic;
        }

        public override void UpdateGraphicBySeason()
        {
            SetGraphic(SeasonManager.GetSeasonGraphic(World.Season, OriginalGraphic));
            AllowedToDraw = !GameObjectHelper.IsNoDrawable(Graphic);
            IsVegetation = StaticFilters.IsVegetation(Graphic);
        }

        public override void UpdateTextCoordsV()
        {
            if (TextContainer == null)
                return;

            var last = (TextObject) TextContainer.Items;

            while (last?.Next != null)
                last = (TextObject) last.Next;

            if (last == null)
                return;

            int offY = 0;

            int startX = ProfileManager.Current.GameWindowPosition.X + 6;
            int startY = ProfileManager.Current.GameWindowPosition.Y + 6;
            var scene = Client.Game.GetScene<GameScene>();
            float scale = scene?.Scale ?? 1;
            int x = RealScreenPosition.X;
            int y = RealScreenPosition.Y;

            x += 22;
            y += 44;

            var texture = ArtLoader.Instance.GetTexture(Graphic);

            if (texture != null)
                y -= (texture.ImageRectangle.Height >> 1);

            x = (int)(x / scale);
            y = (int)(y / scale);

            for (; last != null; last = (TextObject) last.Previous)
            {
                if (last.RenderedText != null && !last.RenderedText.IsDestroyed)
                {
                    if (offY == 0 && last.Time < Time.Ticks)
                        continue;

                    last.OffsetY = offY;
                    offY += last.RenderedText.Height;

                    last.RealScreenPosition.X = startX + (x - (last.RenderedText.Width >> 1));
                    last.RealScreenPosition.Y = startY + (y - offY);
                }
            }

            FixTextCoordinatesInScreen();
        }

        public override void Destroy()
        {
            if (IsDestroyed)
                return;
            base.Destroy();
            _pool.ReturnOne(this);
        }
    }
}