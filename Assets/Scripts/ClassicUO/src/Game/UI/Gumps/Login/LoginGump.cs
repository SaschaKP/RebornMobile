

using System;
using System.Diagnostics;
using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;
using ClassicUO.Input;
using Microsoft.Xna.Framework;
using SDL2;

namespace ClassicUO.Game.UI.Gumps.Login
{
    internal class LoginGump : Gump
    {
        private readonly ushort _buttonNormal;
        private readonly ushort _buttonOver;
        private readonly Checkbox _checkboxAutologin;
        private readonly Checkbox _checkboxSaveAccount;
        private readonly Button _nextArrow0;
        private readonly StbTextBox _textboxAccount;
        private readonly PasswordStbTextBox _passwordFake;

        private float _time;

        public LoginGump(LoginScene scene) : base(0, 0)
        {
            CanCloseWithRightClick = false;

            AcceptKeyboardInput = false;

            int offsetX, offsetY, offtextY;
            byte font;
            ushort hue;

            _buttonNormal = 0x15A4;
            _buttonOver = 0x15A5;
            const ushort HUE = 0x0386;

            if (Client.Version >= ClientVersion.CV_500A)
#if UNITY_EDITOR || UNITY_IOS
                Add(new GumpPic(0, 0, 0x2327, 0));
#else
                Add(new GumpPic(0, 0, 0x2329, 0));
#endif

            //UO Flag
            Add(new GumpPic(0, 4, 0x15A0, 0) { AcceptKeyboardInput = false });

            Add(new Button((int)Buttons.Account, 0x1E8, 0x1E7, 0x1E6, " REGISTER", 3, false, 0x7A6, 0x3F) { X = 2, Y = 148, ButtonAction = ButtonAction.Activate, FontCenter = true });
            Add(new Button((int)Buttons.Forum, 0x1E2, 0x1E1, 0x1E0, "FORUM", 3, false, 0x7A6, 0x3F) { X = 6, Y = 206, ButtonAction = ButtonAction.Activate, FontCenter = true });
            Add(new Button((int)Buttons.Discord, 0x1E5, 0x1E4, 0x1E3, "DISCORD", 3, false, 0x7A6, 0x3F) { X = 2, Y = 244, ButtonAction = ButtonAction.Activate, FontCenter = true });
            // Quit Button
            Add(new Button((int)Buttons.Quit, 0x1589, 0x158B, 0x158A)
            {
                X = 555,
                Y = 4,
                ButtonAction = ButtonAction.Activate
            });

            //Login Panel
            Add(new ResizePic(0x13BE)
            {
                X = 128,
                Y = 288,
                Width = 451,
                Height = 157
            });

            if (Client.Version < ClientVersion.CV_500A)
                Add(new GumpPic(286, 45, 0x058A, 0));

            Add(new Label("Log in to the server", false, HUE, font: 2)
            {
                X = 253,
                Y = 305
            });

            Add(new Label("Account Name", false, HUE, font: 2)
            {
                X = 183,
                Y = 345
            });

            Add(new Label("Password", false, HUE, font: 2)
            {
                X = 183,
                Y = 385
            });

            // Arrow Button
            Add(_nextArrow0 = new Button((int) Buttons.NextArrow, 0x15A4, 0x15A6, 0x15A5)
            {
                X = 610,
                ButtonAction = ButtonAction.Activate
            });
            if (UserPreferences.ShowModifierKeyButtons.CurrentValue == 0)
                _nextArrow0.Y = 445;
            else
                _nextArrow0.Y = 420;

            offsetX = 328;
            offsetY = 343;
            offtextY = 40;

            Add(new Label($"Client Version {Settings.GlobalSettings.ClientVersion}", false, 0x034E, font: 9)
            {
                X = 286,
                Y = 453
            });

            string patch = UnityEngine.PlayerPrefs.GetString(UOItaliaDownloader.UOITALIA_VERSION_PREF_KEY, "0");
            Add(new Label($"Mobile Version {UnityEngine.Application.version} (patch {patch})", false, 0x034E, font: 9)
            {
                X = 220,
                Y = 465
            });


            Add(_checkboxAutologin = new Checkbox(0x00D2, 0x00D3, "Autologin", 1, 0x0386, false)
            {
                X = 150,
                Y = 417
            });

            Add(_checkboxSaveAccount = new Checkbox(0x00D2, 0x00D3, "Save Account", 1, 0x0386, false)
            {
                X = _checkboxAutologin.X + _checkboxAutologin.Width + 10,
                Y = 417
            });

            font = 1;
            hue = 0x0386;
 
            //Upscale arrow button on mobile
            UpscaleNextArrow();

            // Account Text Input Background
            Add(new ResizePic(0x0BB8)
            {
                X = offsetX,
                Y = offsetY,
                Width = 210,
                Height = 30
            });

            // Password Text Input Background
            Add(new ResizePic(0x0BB8)
            {
                X = offsetX,
                Y = offsetY + offtextY,
                Width = 210,
                Height = 30
            });

            offsetX += 7;

            // Text Inputs
            Add(_textboxAccount = new StbTextBox(5, 16, 190, false, hue: 0x034F)
            {
                X = offsetX,
                Y = offsetY,
                Width = 190,
                Height = 25,
            });

            _textboxAccount.SetText(Settings.GlobalSettings.Username);

            Add(_passwordFake = new PasswordStbTextBox(5, 16, 190, false, hue: 0x034F)
            {
                X = offsetX,
                Y = offsetY + offtextY + 2,
                Width = 190,
                Height = 25,
            });

            _passwordFake.RealText = Crypter.Decrypt(Settings.GlobalSettings.Password);

            _checkboxSaveAccount.IsChecked = Settings.GlobalSettings.SaveAccount;
            _checkboxAutologin.IsChecked = Settings.GlobalSettings.AutoLogin;

            var loginmusic_checkbox = new Checkbox(0x00D2, 0x00D3, "Music", font, hue, false)
            {
                X = _checkboxSaveAccount.X + _checkboxSaveAccount.Width + 10,
                Y = 417,
                IsChecked = Settings.GlobalSettings.LoginMusic,
            };
            Add(loginmusic_checkbox);

            var login_music = new HSliderBar(loginmusic_checkbox.X + loginmusic_checkbox.Width + 10, loginmusic_checkbox.Y + 4, 80, 0, 100, Settings.GlobalSettings.LoginMusicVolume, HSliderBarStyle.MetalWidgetRecessedBar, true, font, hue, unicode: false);
            Add(login_music);
            login_music.IsVisible = Settings.GlobalSettings.LoginMusic;

            loginmusic_checkbox.ValueChanged += (sender, e) =>
            {
                Settings.GlobalSettings.LoginMusic = loginmusic_checkbox.IsChecked;
                scene.Audio.UpdateCurrentMusicVolume(true);

                login_music.IsVisible = Settings.GlobalSettings.LoginMusic;
            };
            login_music.ValueChanged += (sender, e) =>
            {
                Settings.GlobalSettings.LoginMusicVolume = login_music.Value;
                scene.Audio.UpdateCurrentMusicVolume(true);
            };


            if (!string.IsNullOrEmpty(_textboxAccount.Text))
                _passwordFake.SetKeyboardFocus();
            else
                _textboxAccount.SetKeyboardFocus();
        }

        private void UpscaleNextArrow()
        {
            //We use a size threshold because for some servers or client versions, the next arrow is actually a Login
            //button and due to its size and position on screen, it doesn't make sense to scale it
            const int sizeThreshold = 30;
            if (UnityEngine.Application.isMobilePlatform && _nextArrow0.Width < sizeThreshold && _nextArrow0.Height < sizeThreshold)
            {
                const float upscaleFactor = 1.5f;
                _nextArrow0.Width = UnityEngine.Mathf.RoundToInt(_nextArrow0.Width * upscaleFactor);
                _nextArrow0.Height = UnityEngine.Mathf.RoundToInt(_nextArrow0.Height * upscaleFactor);
                _nextArrow0.ContainsByBounds = true;
            }
        }


        public override void OnKeyboardReturn(int textID, string text)
        {
            SaveCheckboxStatus();
            LoginScene ls = Client.Game.GetScene<LoginScene>();

            if (ls.CurrentLoginStep == LoginSteps.Main)
                ls.Connect(_textboxAccount.Text, _passwordFake.RealText);
        }

        private void SaveCheckboxStatus()
        {
            Settings.GlobalSettings.SaveAccount = _checkboxSaveAccount.IsChecked;
            Settings.GlobalSettings.AutoLogin = _checkboxAutologin.IsChecked;
        }

        public override void Update(double totalMS, double frameMS)
        {
            if (IsDisposed)
                return;

            base.Update(totalMS, frameMS);

            if (_time < totalMS)
            {
                _time = (float) totalMS + 1000;
                _nextArrow0.ButtonGraphicNormal = _nextArrow0.ButtonGraphicNormal == _buttonNormal ? _buttonOver : _buttonNormal;
                
                //Setting ButtonGraphicNormal resets the button's Width and Height so we need to apply the upscaling again
                UpscaleNextArrow();
            }

            if (_passwordFake.HasKeyboardFocus)
            {
                if (_passwordFake.Hue != 0x0021)
                    _passwordFake.Hue = 0x0021;
            }
            else if (_passwordFake.Hue != 0)
                _passwordFake.Hue = 0;

            if (_textboxAccount.HasKeyboardFocus)
            {
                if (_textboxAccount.Hue != 0x0021)
                    _textboxAccount.Hue = 0x0021;
            }
            else if (_textboxAccount.Hue != 0)
                _textboxAccount.Hue = 0;
        }

        public override void OnButtonClick(int buttonID)
        {
            switch ((Buttons) buttonID)
            {
                case Buttons.NextArrow:
                    SaveCheckboxStatus();
                    if (!_textboxAccount.IsDisposed)
                        Client.Game.GetScene<LoginScene>().Connect(_textboxAccount.Text, _passwordFake.RealText);

                    break;

                case Buttons.Quit:
                    Client.Game.Exit();

                    break;

                case Buttons.Account:
                    if (UnityEngine.Application.isMobilePlatform)
                    {
                        UnityEngine.Application.OpenURL("https://www.uoitalia.net/en/cb-registration");
                    }
                    else
                    {
                        try
                        {
                            Process.Start("https://www.uoitalia.net/en/cb-registration");
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.ToString());
                        }
                    }
                    
                    break;
                case Buttons.Forum:
                    if (UnityEngine.Application.isMobilePlatform)
                    {
                        UnityEngine.Application.OpenURL("https://www.uoitalia.net/en/ultimaonline-community/forum");
                    }
                    else
                    {
                        try
                        {
                            Process.Start("https://www.uoitalia.net/en/ultimaonline-community/forum");
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.ToString());
                        }
                    }
                    break;
                case Buttons.Discord:
                    if (UnityEngine.Application.isMobilePlatform)
                    {
                        UnityEngine.Application.OpenURL("https://discord.gg/g44wjq5");
                    }
                    else
                    {
                        try
                        {
                            Process.Start("https://discord.gg/g44wjq5");
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.ToString());
                        }
                    }
                    break;
            }
        }

        public class PasswordStbTextBox : StbTextBox
        {
            public PasswordStbTextBox(byte font, int max_char_count = -1, int maxWidth = 0, bool isunicode = true, FontStyle style = FontStyle.None, ushort hue = 0, TEXT_ALIGN_TYPE align = TEXT_ALIGN_TYPE.TS_LEFT) : base(font, max_char_count, maxWidth, isunicode, style, hue, align)
            {
                _rendererText = RenderedText.Create(string.Empty, hue, font, isunicode, style, align, maxWidth, 30, false, false, false);
                _rendererCaret = RenderedText.Create("_", hue, font, isunicode, (style & FontStyle.BlackBorder) != 0 ? FontStyle.BlackBorder : FontStyle.None, align: align);
                NoSelection = true;
            }

            internal string RealText
            {
                get
                {
                    return Text;
                }
                set
                {
                    SetText(value);
                }
            }

            protected override void DrawCaret(UltimaBatcher2D batcher, int x, int y)
            {
                if (HasKeyboardFocus)
                {
                    _rendererCaret.Draw(batcher, x + _caretScreenPosition.X, y + _caretScreenPosition.Y);
                }
            }

            private Point _caretScreenPosition;
            protected override void OnMouseDown(int x, int y, MouseButtonType button)
            {
                base.OnMouseDown(x, y, button);
                if (button == MouseButtonType.Left)
                {
                    UpdateCaretScreenPosition();
                }
            }

            protected override void OnKeyDown(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
            {
                base.OnKeyDown(key, mod);
                UpdateCaretScreenPosition();
            }

            public override void Dispose()
            {
                _rendererText?.Destroy();
                _rendererCaret?.Destroy();

                base.Dispose();
            }

            public ushort Hue
            {
                get => _rendererText.Hue;
                set
                {
                    if (_rendererText.Hue != value)
                    {
                        _rendererText.Hue = value;
                        _rendererCaret.Hue = value;

                        _rendererText.CreateTexture();
                        _rendererCaret.CreateTexture();
                    }
                }
            }

            protected override void OnTextInput(string c)
            {
                 base.OnTextInput(c);
            }

            protected override void OnTextChanged()
            {
                if (Text.Length > 0)
                    _rendererText.Text = new string('*', Text.Length);
                else
                    _rendererText.Text = string.Empty;
                base.OnTextChanged();
                UpdateCaretScreenPosition();
            }

            internal override void OnFocusEnter()
            {
                base.OnFocusEnter();
                CaretIndex = Text?.Length ?? 0;
                UpdateCaretScreenPosition();
            }

            private void UpdateCaretScreenPosition()
            {
                _caretScreenPosition = _rendererText.GetCaretPosition(Stb.CursorIndex);
            }

            private RenderedText _rendererText, _rendererCaret;
            public override bool Draw(UltimaBatcher2D batcher, int x, int y)
            {
                Rectangle scissor = ScissorStack.CalculateScissors(Matrix.Identity, x, y, Width, Height);

                if (ScissorStack.PushScissors(batcher.GraphicsDevice, scissor))
                {
                    batcher.EnableScissorTest(true);
                    DrawSelection(batcher, x, y);

                    _rendererText.Draw(batcher, x, y);

                    DrawCaret(batcher, x, y);

                    batcher.EnableScissorTest(false);
                    ScissorStack.PopScissors(batcher.GraphicsDevice);
                }

                return true;
            }
        }


        public enum Buttons
        {
            NextArrow,
            Quit,
            Account,
            Discord,
            Forum
        }
    }
}