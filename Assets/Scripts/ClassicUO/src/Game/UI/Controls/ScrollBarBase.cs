

using System;

using ClassicUO.Input;
using ClassicUO.Utility;

namespace ClassicUO.Game.UI.Controls
{
    abstract class ScrollBarBase : Control
    {
        protected int _value, _minValue, _maxValue;


        public event EventHandler ValueChanged;


        public int Value
        {
            get => _value;
            set
            {
                if (_value == value)
                    return;

                _value = value;

                if (_value < MinValue)
                    _value = MinValue;
                else if (_value > MaxValue)
                    _value = MaxValue;

                ValueChanged.Raise();
            }
        }

        public int MinValue
        {
            get => _minValue;
            set
            {
                if (_minValue == value)
                    return;

                _minValue = value;

                if (_value < _minValue)
                    _value = _minValue;
            }
        }

        public int MaxValue
        {
            get => _maxValue;
            set
            {
                if (_maxValue == value)
                    return;

                if (value < 0)
                    _maxValue = 0;
                else
                    _maxValue = value;

                if (_value > _maxValue)
                    _value = _maxValue;
            }
        }

        public int ScrollStep { get; set; } = 50;





        protected override void OnMouseWheel(MouseEventType delta)
        {
            switch (delta)
            {
                case MouseEventType.WheelScrollUp:
                    Value -= ScrollStep;

                    break;

                case MouseEventType.WheelScrollDown:
                    Value += ScrollStep;

                    break;
            }
        }

        protected int GetSliderYPosition()
        {
            if (MaxValue == MinValue)
                return 0;

            return (int) Math.Round(GetScrollableArea() * ((Value - MinValue) / (float) (MaxValue - MinValue)));
        }

        protected abstract int GetScrollableArea();
    }
}