

using System;

using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;

using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Controls
{
    internal class ScrollBar : ScrollBarBase
    {
        private const int TIME_BETWEEN_CLICKS = 2;
        private bool _btUpClicked, _btDownClicked, _btSliderClicked;
        private Point _clickPosition;
        private Rectangle _rectDownButton, _rectUpButton, _rectSlider, _emptySpace;
        private int _sliderPosition;
        private UOTexture32 _textureSlider;
        private UOTexture32[] _textureUpButton, _textureDownButton, _textureBackground;
        private uint _timeUntilNextClick;

        public ScrollBar(int x, int y, int height)
        {
            Height = height;
            Location = new Point(x, y);
            AcceptMouseInput = true;


            _textureUpButton = new UOTexture32[2];
            _textureUpButton[0] = GumpsLoader.Instance.GetTexture(251);
            _textureUpButton[1] = GumpsLoader.Instance.GetTexture(250);
            _textureDownButton = new UOTexture32[2];
            _textureDownButton[0] = GumpsLoader.Instance.GetTexture(253);
            _textureDownButton[1] = GumpsLoader.Instance.GetTexture(252);
            _textureBackground = new UOTexture32[3];
            _textureBackground[0] = GumpsLoader.Instance.GetTexture(257);
            _textureBackground[1] = GumpsLoader.Instance.GetTexture(256);
            _textureBackground[2] = GumpsLoader.Instance.GetTexture(255);
            _textureSlider = GumpsLoader.Instance.GetTexture(254);
            Width = _textureBackground[0].Width;


            _rectDownButton = new Rectangle(0, Height - _textureDownButton[0].Height, _textureDownButton[0].Width, _textureDownButton[0].Height);
            _rectUpButton = new Rectangle(0, 0, _textureUpButton[0].Width, _textureUpButton[0].Height);
            _rectSlider = new Rectangle((_textureBackground[0].Width - _textureSlider.Width) >> 1, _textureUpButton[0].Height + (int) _sliderPosition, _textureSlider.Width, _textureSlider.Height);
            _emptySpace.X = 0;
            _emptySpace.Y = _textureUpButton[0].Height;
            _emptySpace.Width = _textureSlider.Width;
            _emptySpace.Height = Height - (_textureDownButton[0].Height + _textureUpButton[0].Height);
        }

       
        public override void Update(double totalMS, double frameMS)
        {
            base.Update(totalMS, frameMS);

            if (MaxValue <= MinValue)
                Value = MaxValue = MinValue;

            _sliderPosition = GetSliderYPosition();
            _rectSlider.Y = _textureUpButton[0].Height + (int) _sliderPosition;

            if (_btUpClicked || _btDownClicked)
            {
                if (_timeUntilNextClick < Time.Ticks)
                {
                    _timeUntilNextClick = Time.Ticks + TIME_BETWEEN_CLICKS;

                    if (_btUpClicked)
                        Value -= 1 + _StepChanger;
                    else if (_btDownClicked)
                        Value += 1 + _StepChanger;

                    _StepsDone++;

                    if (_StepsDone % 8 == 0)
                        _StepChanger++;
                }
            }

            for (int i = 0; i < 3; i++)
            {
                if (i == 0)
                    _textureSlider.Ticks = (long) totalMS;

                if (i < 2)
                {
                    _textureUpButton[i].Ticks = (long) totalMS;
                    _textureDownButton[i].Ticks = (long) totalMS;
                }

                _textureBackground[i].Ticks = (long) totalMS;
            }
        }

        public override bool Draw(UltimaBatcher2D batcher, int x, int y)
        {
            if (Height <= 0 || !IsVisible)
                return false;

            ResetHueVector();

            // draw scrollbar background
            int middleHeight = Height - _textureUpButton[0].Height - _textureDownButton[0].Height - _textureBackground[0].Height - _textureBackground[2].Height;

            if (middleHeight > 0)
            {
                batcher.Draw2D(_textureBackground[0], x, y + _textureUpButton[0].Height, ref _hueVector);
                batcher.Draw2DTiled(_textureBackground[1], x, y + _textureUpButton[0].Height + _textureBackground[0].Height, _textureBackground[0].Width, middleHeight, ref _hueVector);
                batcher.Draw2D(_textureBackground[2], x, y + Height - _textureDownButton[0].Height - _textureBackground[2].Height, ref _hueVector);
            }
            else
            {
                middleHeight = Height - _textureUpButton[0].Height - _textureDownButton[0].Height;
                batcher.Draw2DTiled(_textureBackground[1], x, y + _textureUpButton[0].Height, _textureBackground[0].Width, middleHeight, ref _hueVector);
            }

            // draw up button
            batcher.Draw2D(_btUpClicked ? _textureUpButton[1] : _textureUpButton[0], x, y, ref _hueVector);

            // draw down button
            batcher.Draw2D(_btDownClicked ? _textureDownButton[1] : _textureDownButton[0], x, y + Height - _textureDownButton[0].Height, ref _hueVector);

            // draw slider
            if (MaxValue > MinValue && middleHeight > 0)
                batcher.Draw2D(_textureSlider, x + ((_textureBackground[0].Width - _textureSlider.Width) >> 1), (int) (y + _textureUpButton[0].Height + _sliderPosition), ref _hueVector);

            return base.Draw(batcher, x, y);
        }

        protected override int GetScrollableArea()
        {
            return Height - _textureUpButton[0].Height - _textureDownButton[0].Height - _textureSlider.Height;
        }

        protected override void OnMouseDown(int x, int y, MouseButtonType button)
        {
            if (button != MouseButtonType.Left)
                return;

            _timeUntilNextClick = 0;

            if (_rectDownButton.Contains(x, y))
            {
                // clicked on the down button
                _btDownClicked = true;
            }
            else if (_rectUpButton.Contains(x, y))
            {
                // clicked on the up button
                _btUpClicked = true;
            }
            else if (_rectSlider.Contains(x, y))
            {
                // clicked on the slider
                _btSliderClicked = true;
                _clickPosition.X = x;
                _clickPosition.Y = y;
            }
            else if (_emptySpace.Contains(x, y))
            {
                CalculateByPosition(x, y);
            }
        }

        protected override void OnMouseUp(int x, int y, MouseButtonType button)
        {
            if (button != MouseButtonType.Left)
                return;

            _btDownClicked = false;
            _btUpClicked = false;
            _btSliderClicked = false;
            _StepChanger = _StepsDone = 1;
        }

        protected override void OnMouseOver(int x, int y)
        {
            if (_btSliderClicked)
                CalculateByPosition(x, y);
        }

        private void CalculateByPosition(int x, int y)
        {
            if (y != _clickPosition.Y)
            {
                y -= _emptySpace.Y + (_rectSlider.Height >> 1);

                if (y < 0)
                    y = 0;

                int scrollableArea = GetScrollableArea();
                if (y > scrollableArea)
                    y = scrollableArea;

                _sliderPosition = y;
                _clickPosition.X = x;
                _clickPosition.Y = y;

                if (y == 0 && _clickPosition.Y < _textureUpButton[0].Height + (_textureSlider.Height >> 1))
                    _clickPosition.Y = _textureUpButton[0].Height + (_textureSlider.Height >> 1);
                else if (y == scrollableArea && _clickPosition.Y > Height - _textureDownButton[0].Height - (_textureSlider.Height >> 1))
                    _clickPosition.Y = Height - _textureDownButton[0].Height - (_textureSlider.Height >> 1);

                _value = (int) Math.Round (y / (float) scrollableArea * (MaxValue - MinValue) + MinValue);
            }
        }

        public override bool Contains(int x, int y)
        {
            return x >= 0 && x <= Width && y >= 0 && y <= Height;
        }
    }
}