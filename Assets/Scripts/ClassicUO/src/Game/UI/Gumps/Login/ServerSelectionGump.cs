

using System.Linq;

using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Utility.Logging;
using SDL2;

namespace ClassicUO.Game.UI.Gumps.Login
{
    internal class ServerSelectionGump : Gump
    {
        private const ushort SELECTED_COLOR = 0x0021;
        private const ushort NORMAL_COLOR = 0x034F;

        public ServerSelectionGump() : base(0, 0)
        {
            //AddChildren(new LoginBackground(true));

            Add(new Button((int) Buttons.Prev, 0x15A1, 0x15A3, 0x15A2)
            {
                X = 586, Y = 445, ButtonAction = ButtonAction.Activate
            });

            Add(new Button((int) Buttons.Next, 0x15A4, 0x15A6, 0x15A5)
            {
                X = 610, Y = 445, ButtonAction = ButtonAction.Activate
            });

            if (Client.Version >= ClientVersion.CV_500A)
            {
                ushort textColor = 0xFFFF;

                Add(new Label(ClilocLoader.Instance.GetString(1044579), true, textColor, font: 1)
                {
                    X = 155, Y = 70
                }); // "Select which shard to play on:"

                Add(new Label(ClilocLoader.Instance.GetString(1044577), true, textColor, font: 1)
                {
                    X = 400, Y = 70
                }); // "Latency:"

                Add(new Label(ClilocLoader.Instance.GetString(1044578), true, textColor, font: 1)
                {
                    X = 470, Y = 70
                }); // "Packet Loss:"

                Add(new Label(ClilocLoader.Instance.GetString(1044580), true, textColor, font: 1)
                {
                    X = 153, Y = 368
                }); // "Sort by:"
            }
            else
            {
                ushort textColor = 0x0481;

                Add(new Label("Select which shard to play on:", false, textColor, font: 9)
                {
                    X = 155, Y = 70
                });

                Add(new Label("Latency:", false, textColor, font: 9)
                {
                    X = 400, Y = 70
                });

                Add(new Label("Packet Loss:", false, textColor, font: 9)
                {
                    X = 470, Y = 70
                });

                Add(new Label("Sort by:", false, textColor, font: 9)
                {
                    X = 153, Y = 368
                });
            }

            Add(new Button((int) Buttons.SortTimeZone, 0x093B, 0x093C, 0x093D)
            {
                X = 230, Y = 366
            });

            Add(new Button((int) Buttons.SortFull, 0x093E, 0x093F, 0x0940)
            {
                X = 338, Y = 366
            });

            Add(new Button((int) Buttons.SortConnection, 0x0941, 0x0942, 0x0943)
            {
                X = 446, Y = 366
            });

            // World Pic Bg
            Add(new GumpPic(150, 390, 0x0589, 0));

            // Earth
            Add(new Button((int) Buttons.Earth, 0x15E8, 0x15EA, 0x15E9)
            {
                X = 160, Y = 400, ButtonAction = ButtonAction.Activate
            });

            // Sever Scroll Area Bg
            Add(new ResizePic(0x0DAC)
            {
                X = 150, Y = 90, Width = 393 - 14, Height = 271
            });
            // Sever Scroll Area
            ScrollArea scrollArea = new ScrollArea(150, 90, 393, 271, true);
            LoginScene loginScene = Client.Game.GetScene<LoginScene>();

            scrollArea.ScissorRectangle.Y = 16;
            scrollArea.ScissorRectangle.Height = -(scrollArea.ScissorRectangle.Y + 32);

            foreach (ServerListEntry server in loginScene.Servers)
            {
                scrollArea.Add(new ServerEntryGump(server, 5, NORMAL_COLOR, SELECTED_COLOR));
            }

            Add(scrollArea);

            if (loginScene.Servers.Length != 0)
            {
                int index = Settings.GlobalSettings.LastServerNum - 1;

                if (index < 0 || index >= loginScene.Servers.Length)
                {
                    index = 0;
                }

                Add(new Label(loginScene.Servers[index].Name, false, 0x0481, font: 9)
                {
                    X = 243,
                    Y = 420
                });
            }

            AcceptKeyboardInput = true;
            CanCloseWithRightClick = false;
        }

        public override void OnButtonClick(int buttonID)
        {
            LoginScene loginScene = Client.Game.GetScene<LoginScene>();

            if (buttonID >= (int) Buttons.Server)
            {
                int index = buttonID - (int) Buttons.Server;
                loginScene.SelectServer((byte) index);
            }
            else
            {
                switch ((Buttons) buttonID)
                {
                    case Buttons.Next:
                    case Buttons.Earth:

                        if (loginScene.Servers.Length != 0)
                        {
                            int index = Settings.GlobalSettings.LastServerNum;

                            if (index <= 0 || index > loginScene.Servers.Length)
                            {
                                Log.Warn( $"Wrong server index: {index}");

                                index = 1;
                            }

                            loginScene.SelectServer((byte) loginScene.Servers[index - 1].Index);
                        }

                        break;

                    case Buttons.Prev:
                        loginScene.StepBack();

                        break;
                }
            }
        }

        protected override void OnKeyDown(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
        {
            if (key == SDL.SDL_Keycode.SDLK_RETURN || key == SDL.SDL_Keycode.SDLK_KP_ENTER)
            {
                LoginScene loginScene = Client.Game.GetScene<LoginScene>();

                if (loginScene.Servers.Any())
                {
                    int index = Settings.GlobalSettings.LastServerNum;

                    if (index <= 0 || index > loginScene.Servers.Length)
                    {
                        Log.Warn( $"Wrong server index: {index}");

                        index = 1;
                    }

                    loginScene.SelectServer((byte) loginScene.Servers[index - 1].Index);
                }
            }
        }

        private enum Buttons
        {
            Prev,
            Next,
            SortTimeZone,
            SortFull,
            SortConnection,
            Earth,
            Server = 99
        }

        private class ServerEntryGump : ScrollAreaItem
        {
            private readonly int _buttonId;

            private readonly HoveredLabel _serverName;
            private readonly HoveredLabel _server_ping;
            private readonly HoveredLabel _server_packet_loss;
            private readonly ServerListEntry _entry;

            public ServerEntryGump(ServerListEntry entry, byte font, ushort normal_hue, ushort selected_hue)
            {
                _entry = entry;

                _buttonId = entry.Index;

                Add(_serverName = new HoveredLabel(entry.Name, false, normal_hue, selected_hue, selected_hue, font: font)
                {
                    X = 74,
                    AcceptMouseInput = false
                });
                Add(_server_ping = new HoveredLabel("-", false, normal_hue, selected_hue, selected_hue, font: font)
                {
                    X = 250,
                    AcceptMouseInput = false
                });
                Add(_server_packet_loss = new HoveredLabel("-", false, normal_hue, selected_hue, selected_hue, font: font)
                {
                    X = 320,
                    AcceptMouseInput = false
                });


                AcceptMouseInput = true;
                Width = 393;
                Height = 25;

                WantUpdateSize = false;
            }

            protected override void OnMouseEnter(int x, int y)
            {
                base.OnMouseEnter(x, y);

                _serverName.IsSelected = true;
                _server_packet_loss.IsSelected = true;
                _server_ping.IsSelected = true;
            }

            protected override void OnMouseExit(int x, int y)
            {
                base.OnMouseExit(x, y);

                _serverName.IsSelected = false;
                _server_packet_loss.IsSelected = false;
                _server_ping.IsSelected = false;
            }

            protected override void OnMouseUp(int x, int y, MouseButtonType button)
            {
                if (button == MouseButtonType.Left) 
                    OnButtonClick((int)Buttons.Server + _buttonId);
            }
        }
    }
}