


namespace ClassicUO.Game.UI.Controls
{
    enum UILayer
    {
        Over,
        Default,
        Under
    }


    internal class GumpControlInfo
    {
        public GumpControlInfo(Control control)
        {
            Control = control;
        }

        public UILayer Layer { get; set; } = UILayer.Default;

        public bool IsModal { get; set; }

        public bool ModalClickOutsideAreaClosesThisControl { get; set; }

        public Control Control { get; }
    }
}