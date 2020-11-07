

namespace ClassicUO.Renderer
{
    internal static class Fonts
    {
        public static SpriteFont Regular { get; private set; }
        public static SpriteFont Bold { get; private set; }
        public static SpriteFont Map1 { get; private set; }
        public static SpriteFont Map2 { get; private set; }
        public static SpriteFont Map3 { get; private set; }
        public static SpriteFont Map4 { get; private set; }
        public static SpriteFont Map5 { get; private set; }
        public static SpriteFont Map6 { get; private set; }

        static Fonts()
        {
            Regular = SpriteFont.Create("regular_font");
            Bold = SpriteFont.Create("bold_font");

            Map1 = SpriteFont.Create("map1_font");
            Map2 = SpriteFont.Create("map2_font");
            Map3 = SpriteFont.Create("map3_font");
            Map4 = SpriteFont.Create("map4_font");
            Map5 = SpriteFont.Create("map5_font");
            Map6 = SpriteFont.Create("map6_font");
        }
    }
}