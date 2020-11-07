

using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Renderer;

namespace ClassicUO.Game.UI.Gumps.Login
{
    internal class LoginBackground : Gump
    {
        public LoginBackground() : base(0, 0)
        {
            if (Client.Version >= ClientVersion.CV_706400)
            {
                // Background
                Add(new GumpPicTiled(0, 0, 640, 480, 0x0150) { AcceptKeyboardInput = false });
                // UO Flag
                Add(new GumpPic(0, 4, 0x0151, 0) { AcceptKeyboardInput = false });
            }
            else
            {
                // Background
                Add(new GumpPicTiled(0, 0, 640, 480, 0x0E14) { AcceptKeyboardInput = false });
                // Border
                Add(new GumpPic(0, 0, 0x157C, 0) { AcceptKeyboardInput = false });
                // UO Flag
                Add(new GumpPic(0, 4, 0x15A0, 0) { AcceptKeyboardInput = false });

                // Quit Button
                Add(new Button(0, 0x1589, 0x158B, 0x158A)
                {
                    X = 555,
                    Y = 4,
                    ButtonAction = ButtonAction.Activate,
                    AcceptKeyboardInput = false
                });
            }


            CanCloseWithEsc = false;
            CanCloseWithRightClick = false;
            AcceptKeyboardInput = false;

            ControlInfo.Layer = UILayer.Under;
        }


        public override void OnButtonClick(int buttonID)
        {
            Client.Game.Exit();
        }
    }
}