

using System.Collections.Generic;
using System.Linq;

using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;

namespace ClassicUO.Game.UI.Controls
{
    internal class NiceButton : HitBox
    {
        private readonly ButtonAction _action;
        private readonly int _groupnumber;
        private bool _isSelected;
        internal Label TextLabel { get; }

        public NiceButton(int x, int y, int w, int h, ButtonAction action, string text, int groupnumber = 0, TEXT_ALIGN_TYPE align = TEXT_ALIGN_TYPE.TS_CENTER) : base(x, y, w, h)
        {
            _action = action;
            Add(TextLabel = new Label(text, true, 999, w, 0xFF, FontStyle.BlackBorder | FontStyle.Cropped, align));
            TextLabel.Y = (h - TextLabel.Height) >> 1;
            _groupnumber = groupnumber;
        }

        public int ButtonParameter { get; set; }

        public bool IsSelectable { get; set; } = true;

        public bool IsSelected
        {
            get => _isSelected && IsSelectable;
            set
            {
                if (!IsSelectable)
                    return;

                _isSelected = value;

                if (value)
                {
                    Control p = Parent;

                    if (p == null)
                        return;

                    IEnumerable<NiceButton> list;

                    if (p is ScrollAreaItem)
                    {
                        p = p.Parent;
                        list = p.FindControls<ScrollAreaItem>().SelectMany(s => s.Children.OfType<NiceButton>());
                    }
                    else
                        list = p.FindControls<NiceButton>();

                    foreach (var b in list)
                        if (b != this && b._groupnumber == _groupnumber)
                            b.IsSelected = false;
                }
            }
        }

        internal static NiceButton GetSelected(Control p, int group)
        {
            IEnumerable<NiceButton> list = p is ScrollArea ? p.FindControls<ScrollAreaItem>().SelectMany(s => s.Children.OfType<NiceButton>()) : p.FindControls<NiceButton>();

            foreach (var b in list)
                if (b._groupnumber == group && b.IsSelected)
                    return b;

            return null;
        }

        protected override void OnMouseUp(int x, int y, MouseButtonType button)
        {
            if (button == MouseButtonType.Left)
            {
                IsSelected = true;

                if (_action == ButtonAction.SwitchPage)
                    ChangePage(ButtonParameter);
                else
                    OnButtonClick(ButtonParameter);
            }
        }

        public override bool Draw(UltimaBatcher2D batcher, int x, int y)
        {
            if (IsSelected)
            {
                ResetHueVector();
                ShaderHueTranslator.GetHueVector(ref _hueVector, 0, false, Alpha);
                batcher.Draw2D(_texture, x, y, 0, 0, Width, Height, ref _hueVector);
            }

            return base.Draw(batcher, x, y);
        }
    }
}