

using ClassicUO.Configuration;
using ClassicUO.Game.Managers;
using ClassicUO.IO.Resources;

namespace ClassicUO.Game.UI.Controls
{
    internal class InfoBarBuilderControl : Control
    {
        public string LabelText { get { return infoLabel.Text; } }
        public InfoBarVars Var { get { return (InfoBarVars) varStat.SelectedIndex; } }
        public ushort Hue { get { return labelColor.Hue; } }

        private StbTextBox infoLabel;
        private Combobox varStat;
        private ClickableColorBox labelColor;

        public InfoBarBuilderControl(InfoBarItem item)
        {
            infoLabel = new StbTextBox(0xFF, 10, 80) { X = 5, Y = 0, Width = 130, Height = 30};
            infoLabel.SetText(item.label);

            string[] dataVars = InfoBarManager.GetVars();
            varStat = new Combobox(200, 0, 170, dataVars, (int) item.var);

            uint color = 0xFF7F7F7F;

            if (item.hue != 0xFFFF)
                color = HuesLoader.Instance.GetPolygoneColor(12, item.hue);

            labelColor = new ClickableColorBox(150, 0, 13, 14, item.hue, color);

            NiceButton deleteButton = new NiceButton(390, 0, 60, 25, ButtonAction.Activate, "Delete") { ButtonParameter = 999 };
            deleteButton.MouseUp += (sender, e) =>
            {
                Dispose();
            };

            Add(new ResizePic(0x0BB8) { X = infoLabel.X - 5, Y = 0, Width = infoLabel.Width + 10, Height = infoLabel.Height - 6 });
            Add(infoLabel);
            Add(varStat);
            Add(labelColor);
            Add(deleteButton);
        }

    }

}
