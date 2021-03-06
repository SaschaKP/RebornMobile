

using System;

using ClassicUO.Configuration;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;

using Microsoft.Xna.Framework;

namespace ClassicUO.Game.GameObjects
{
    internal partial class Multi
    {
        private int _canBeTransparent;

        public bool IsFromTarget;

        public override bool TransparentTest(int z)
        {
            bool r = true;

            if (Z <= z - ItemData.Height)
                r = false;
            else if (z < Z && (_canBeTransparent & 0xFF) == 0)
                r = false;

            return r;
        }

        public override bool Draw(UltimaBatcher2D batcher, int posX, int posY)
        {
            if (!AllowedToDraw || IsDestroyed)
                return false;

            ResetHueVector();

            ushort hue = Hue;

            if (State != 0)
            {
                if ((State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_IGNORE_IN_RENDER) != 0)
                    return false;

                if ((State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_INCORRECT_PLACE) != 0)
                {
                    hue = 0x002B;
                }

                if ((State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_TRANSPARENT) != 0)
                {
                    if (AlphaHue >= 192)
                    {
                        AlphaHue = 0xFF;
                    }
                    else
                        ProcessAlpha(192);
                }
            }

            ResetHueVector();

            ushort graphic = Graphic;
            bool partial = ItemData.IsPartialHue;

            if (ProfileManager.Current.HighlightGameObjects && SelectedObject.LastObject == this)
            {
                hue = Constants.HIGHLIGHT_CURRENT_OBJECT_HUE;
                partial = false;
            }
            else if (ProfileManager.Current.NoColorObjectsOutOfRange && Distance > World.ClientViewRange)
            {
                hue = Constants.OUT_RANGE_COLOR;
                partial = false;
            }
            else if (World.Player.IsDead && ProfileManager.Current.EnableBlackWhiteEffect)
            {
                hue = Constants.DEAD_RANGE_COLOR;
                partial = false;
            }

            ShaderHueTranslator.GetHueVector(ref HueVector, hue, partial, 0);

            //Engine.DebugInfo.MultiRendered++;

            if (IsFromTarget)
                HueVector.Z = 0.5f;

            posX += (int) Offset.X;
            posY += (int) (Offset.Y + Offset.Z);

            if (AlphaHue != 255)
                HueVector.Z = 1f - AlphaHue / 255f;

            DrawStaticAnimated(batcher, graphic, posX, posY, ref HueVector, ref DrawTransparent);

            if (ItemData.IsLight)
            {
                Client.Game.GetScene<GameScene>()
                      .AddLight(this, this, posX + 22, posY + 22);
            }

            if (!(SelectedObject.Object == this || IsFromTarget ||
                  (FoliageIndex != -1 &&
                   Client.Game.GetScene<GameScene>().FoliageIndex == FoliageIndex)))
            {
                if (State != 0)
                {
                    if ((State & (CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_IGNORE_IN_RENDER |
                                  CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_PREVIEW)) != 0)
                        return true;
                }
                 
                if (DrawTransparent)
                {
                    return true;
                }

                ref var index = ref ArtLoader.Instance.GetValidRefEntry(graphic + 0x4000);

                posX -= index.Width;
                posY -= index.Height;

                if (SelectedObject.IsPointInStatic(ArtLoader.Instance.GetTexture(graphic), posX, posY))
                    SelectedObject.Object = this;
            }

            return true;
        }
    }
}