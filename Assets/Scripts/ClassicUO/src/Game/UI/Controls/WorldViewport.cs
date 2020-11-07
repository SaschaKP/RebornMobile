

using ClassicUO.Configuration;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Input;
using ClassicUO.Renderer;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Controls
{
    internal class WorldViewport : Control
    {
        private readonly BlendState _darknessBlend = new BlendState
        {
            ColorSourceBlend = Blend.Zero,
            ColorDestinationBlend = Blend.SourceColor,

            ColorBlendFunction = BlendFunction.Add
        };

        private readonly BlendState _altLightsBlend = new BlendState
        {
            ColorSourceBlend = Blend.DestinationColor,
            ColorDestinationBlend = Blend.One,

            ColorBlendFunction = BlendFunction.Add
        };

        private readonly GameScene _scene;

        private XBREffect _xBR;

        public WorldViewport(GameScene scene, int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            _scene = scene;
            AcceptMouseInput = true;
        }

        public override bool Draw(UltimaBatcher2D batcher, int x, int y)
        {
            if (_scene.ViewportTexture == null)
                return false;

            Rectangle rectangle = ScissorStack.CalculateScissors(Matrix.Identity, x, y, Width, Height);

            if (ScissorStack.PushScissors(batcher.GraphicsDevice, rectangle))
            {
                batcher.EnableScissorTest(true);

                ResetHueVector();

                if (ProfileManager.Current != null && ProfileManager.Current.UseXBR)
                {
                    // draw regular world
                    if (_xBR == null)
                    {
                        _xBR = new XBREffect(batcher.GraphicsDevice);
                    }

                    _xBR.SetSize(_scene.ViewportTexture.Width, _scene.ViewportTexture.Height);

                    batcher.End();

                    batcher.Begin(_xBR);
                    batcher.Draw2D(_scene.ViewportTexture, x, y, Width, Height, ref _hueVector);
                    batcher.End();

                    batcher.Begin();
                }
                else
                    batcher.Draw2D(_scene.ViewportTexture, x, y, Width, Height, ref _hueVector);


                // draw lights
                // if (_scene.UseAltLights)
                // {
                //     batcher.SetBlendState(_altLightsBlend);
                //     _hueVector.Z = 0.5f;
                //     batcher.Draw2D(_scene.LightRenderTarget, x, y, Width, Height, ref _hueVector);
                //     _hueVector.Z = 0;
                //     batcher.SetBlendState(null);
                // }
                // else if (_scene.UseLights)
                // {
                //     batcher.SetBlendState(_darknessBlend);
                //     batcher.Draw2D(_scene.LightRenderTarget, x, y, Width, Height, ref _hueVector);
                //     batcher.SetBlendState(null);
                // }

                // draw overheads
                _scene.DrawSelection(batcher, x, y);
                _scene.DrawOverheads(batcher, x, y);

                base.Draw(batcher, x, y);

                batcher.EnableScissorTest(false);
                ScissorStack.PopScissors(batcher.GraphicsDevice);
            }

            return true;
        }


        public override void Dispose()
        {
            _xBR?.Dispose();
            _darknessBlend?.Dispose();
            _altLightsBlend?.Dispose();
            base.Dispose();
        }

        protected override void OnMouseUp(int x, int y, MouseButtonType button)
        {
            if (!UIManager.IsMouseOverWorld && UIManager.MouseOverControl != null)
            {
                var p = UIManager.MouseOverControl.GetFirstControlAcceptKeyboardInput();
                p?.SetKeyboardFocus();
            }
            else
            {
                if (UIManager.KeyboardFocusControl != UIManager.SystemChat.TextBoxControl)
                {
                    UIManager.KeyboardFocusControl = UIManager.SystemChat.TextBoxControl;
                }

                //if (!(UIManager.KeyboardFocusControl is TextBox tb && tb.Parent is WorldViewportGump))
                //    Parent.GetFirstControlAcceptKeyboardInput()?.SetKeyboardFocus();
            }

            base.OnMouseUp(x, y, button);
        }
    }
}