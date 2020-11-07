

using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Renderer;

namespace ClassicUO.Game.UI.Gumps
{
    abstract class TextContainerGump : Gump
    {
        protected TextContainerGump(uint local, uint server) : base(local, server)
        {

        }

        public TextRenderer TextRenderer { get; } = new TextRenderer();


        public void AddText(TextObject msg)
        {
            if (msg == null)
                return;

            msg.Time = Time.Ticks + 4000;
           
            TextRenderer.AddMessage(msg);
        }

        public override void Update(double totalMS, double frameMS)
        {
            base.Update(totalMS, frameMS);
            TextRenderer.Update(totalMS, frameMS);
        }

        public override void Dispose()
        {
            TextRenderer.UnlinkD();

            //TextRenderer.Clear();
            base.Dispose();
        }


        public override bool Draw(UltimaBatcher2D batcher, int x, int y)
        {
            base.Draw(batcher, x, y);

            //TextRenderer.MoveToTopIfSelected();
            TextRenderer.ProcessWorldText(true);
            TextRenderer.Draw(batcher, x, y, -1, true);
            return true;
        }
    }
}
