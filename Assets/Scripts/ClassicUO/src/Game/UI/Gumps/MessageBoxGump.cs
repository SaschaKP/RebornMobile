

using System;

using ClassicUO.Game.UI.Controls;
using ClassicUO.Renderer;

namespace ClassicUO.Game.UI.Gumps
{
    internal class MessageBoxGump : Gump
    {
        private readonly Action<bool> _action;

        public MessageBoxGump(int w, int h, string message, Action<bool> action, bool hasBackground = false) : base(0, 0)
        {
            CanMove = true;
            CanCloseWithRightClick = false;
            CanCloseWithEsc = false;
            AcceptMouseInput = false;

            ControlInfo.IsModal = true;
            ControlInfo.Layer = UILayer.Over;
            WantUpdateSize = false;

            Width = w;
            Height = h;
            _action = action;

            Add(new ResizePic(0x0A28)
            {
                Width = w, Height = h
            });

            if (hasBackground)
            {
                ResizePic background = new ResizePic(3000)
                {
                    X = X + 30,
                    Y = Y + 40,
                    Width = Width - 60,
                    Height = Height - 100
                };

                Add(background);
            }

            Add(new Label(message, false, 0x0386, Width - 90, 1)
            {
                X = 40,
                Y = 45
            });

            X = (Client.Game.Window.ClientBounds.Width - Width) >> 1;
            Y = (Client.Game.Window.ClientBounds.Height - Height) >> 1;

            // OK
            Button b;

            Add(b = new Button(0, 0x0481, 0x0483, 0x0482)
            {
                Y = Height - 45,
                ButtonAction = ButtonAction.Activate
            });

            b.X = (Width - b.Width) >> 1;

            WantUpdateSize = false;
        }

        public override void OnButtonClick(int buttonID)
        {
            switch (buttonID)
            {
                case 0:
                    _action?.Invoke(true);
                    Dispose();

                    break;
            }
        }
    }


    internal class EntryDialog : Gump
    {
        private readonly Action<string> _action;
        private readonly StbTextBox _textBox;

        public EntryDialog(int w, int h, string message, Action<string> action) : base(0, 0)
        {
            CanMove = false;
            CanCloseWithRightClick = false;
            CanCloseWithEsc = false;
            AcceptMouseInput = false;

            ControlInfo.IsModal = true;
            ControlInfo.Layer = UILayer.Over;
            WantUpdateSize = false;

            Width = w;
            Height = h;
            _action = action;

            Add(new ResizePic(0x0A28)
            {
                Width = w,
                Height = h
            });

            Label l;

            Add(l = new Label(message, false, 0x0386, Width - 90, 1)
            {
                X = 40,
                Y = 45
            });

            Add(new ResizePic(0x0BB8)
            {
                X = 40,
                Y = 45 + l.Height + 5,
                Width = w - 90,
                Height = 25
            });

            int ww = w - 94;

            _textBox = new StbTextBox(0xFF, -1, ww, true, FontStyle.BlackBorder | FontStyle.Fixed)
            {
                X = 42,
                Y = 45 + l.Height + 7,
                Width = ww,
                Height = 25
            };

            Add(_textBox);

            X = (Client.Game.Window.ClientBounds.Width - Width) >> 1;
            Y = (Client.Game.Window.ClientBounds.Height - Height) >> 1;


            // OK
            Button b;

            Add(b = new Button(0, 0x0481, 0x0482, 0x0483)
            {
                Y = Height - 45,
                ButtonAction = ButtonAction.Activate
            });

            b.X = (Width - b.Width) >> 1;
        }

        public override void OnButtonClick(int buttonID)
        {
            switch (buttonID)
            {
                case 0:
                    _action?.Invoke(_textBox.Text);
                    Dispose();

                    break;
            }
        }
    }
}