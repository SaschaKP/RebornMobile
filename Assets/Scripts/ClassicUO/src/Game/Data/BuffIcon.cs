

using System;

namespace ClassicUO.Game.Data
{
    internal class BuffIcon : IEquatable<BuffIcon>
    {
        public BuffIcon(BuffIconType type, ushort graphic, long timer, string text)
        {
            Type = type;
            Graphic = graphic;
            Timer = timer;
            Text = text;
        }

        public readonly BuffIconType Type;

        public readonly ushort Graphic;

        public readonly long Timer;

        public readonly string Text;

        public bool Equals(BuffIcon other)
        {
            return other != null && Type == other.Type;
        }
    }
}