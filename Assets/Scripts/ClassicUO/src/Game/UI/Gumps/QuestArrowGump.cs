

using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;

namespace ClassicUO.Game.UI.Gumps
{
    internal class QuestArrowGump : Gump
    {
        private GumpPic _arrow;
        private Direction _direction;
        private int _mx;
        private int _my;
        private bool _needHue;
        private float _timer;

        public QuestArrowGump(uint serial, int mx, int my) : base(serial, serial)
        {
            CanMove = false;
            CanCloseWithRightClick = false;

            AcceptMouseInput = true;

            SetRelativePosition(mx, my);
            WantUpdateSize = false;
        }

        public void SetRelativePosition(int x, int y)
        {
            _mx = x;
            _my = y;
        }

        public override void Update(double totalMS, double frameMS)
        {
            base.Update(totalMS, frameMS);

            if (!World.InGame) Dispose();

            var scene = Client.Game.GetScene<GameScene>();

            if (IsDisposed || ProfileManager.Current == null || scene == null)
                return;

            Direction dir = (Direction) GameCursor.GetMouseDirection(World.Player.X, World.Player.Y, _mx, _my, 0);
            ushort gumpID = (ushort) (0x1194 + ((int) dir + 1) % 8);

            if (_direction != dir || _arrow == null)
            {
                _direction = dir;

                if (_arrow == null)
                    Add(_arrow = new GumpPic(0, 0, gumpID, 0));
                else
                    _arrow.Graphic = gumpID;

                Width = _arrow.Width;
                Height = _arrow.Height;
            }

            var scale = scene.Scale;


            int gox = _mx - World.Player.X;
            int goy = _my - World.Player.Y;


            int x = (ProfileManager.Current.GameWindowPosition.X + (ProfileManager.Current.GameWindowSize.X >> 1)) + 6 + ((gox - goy) * (int) (22 / scale)) - (int) ((_arrow.Width / 2f) / scale);
            int y = (ProfileManager.Current.GameWindowPosition.Y + (ProfileManager.Current.GameWindowSize.Y >> 1)) + 6 + ((gox + goy) * (int) (22 / scale)) + (int) ((_arrow.Height) / scale);

            x -= (int) (World.Player.Offset.X / scale);
            y -= (int) (((World.Player.Offset.Y - World.Player.Offset.Z) + (World.Player.Z >> 2)) / scale);

         
            if (x < ProfileManager.Current.GameWindowPosition.X)
                x = ProfileManager.Current.GameWindowPosition.X;
            else if (x > ProfileManager.Current.GameWindowPosition.X + ProfileManager.Current.GameWindowSize.X - _arrow.Width)
                x = ProfileManager.Current.GameWindowPosition.X + ProfileManager.Current.GameWindowSize.X - _arrow.Width;


            if (y < ProfileManager.Current.GameWindowPosition.Y)
                y = ProfileManager.Current.GameWindowPosition.Y;
            else if (y > ProfileManager.Current.GameWindowPosition.Y + ProfileManager.Current.GameWindowSize.Y - _arrow.Height)
                y = ProfileManager.Current.GameWindowPosition.Y + ProfileManager.Current.GameWindowSize.Y - _arrow.Height;

            X = x;
            Y = y;

            if (_timer < Time.Ticks)
            {
                _timer = Time.Ticks + 1000;
                _needHue = !_needHue;
            }

            _arrow.Hue = (ushort) (_needHue ? 0 : 0x21);
        }


        protected override void OnMouseUp(int x, int y, MouseButtonType button)
        {
            var leftClick = button == MouseButtonType.Left;
            var rightClick = button == MouseButtonType.Right;

            if (leftClick || rightClick)
                GameActions.QuestArrow(rightClick);
        }

        public override bool Contains(int x, int y)
        {
            if (_arrow == null)
                return true;

            return _arrow.Contains(x, y);
        }
    }
}