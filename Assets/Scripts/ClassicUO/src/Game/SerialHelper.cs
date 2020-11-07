

using System.Globalization;
using System.Runtime.CompilerServices;

namespace ClassicUO.Game
{
    static class SerialHelper
    {
        [MethodImpl(256)]
        public static bool IsValid(uint serial) => serial > 0 && serial < 0x80000000;

        [MethodImpl(256)]
        public static bool IsMobile(uint serial) => serial > 0 && serial < 0x40000000;

        [MethodImpl(256)]
        public static bool IsItem(uint serial) => serial >= 0x40000000 && serial < 0x80000000;

        [MethodImpl(256)]
        public static bool IsValidLocalGumpSerial(uint serial) => serial >= Constants.JOURNAL_LOCALSERIAL && serial < 0xFFFF_FFFF;

        public static uint Parse(string str)
        {
            if (str.StartsWith("0x"))
                return uint.Parse(str.Remove(0, 2), NumberStyles.HexNumber);

            if (str.Length > 1 && str[0] == '-')
                return (uint) int.Parse(str);

            return uint.Parse(str);
        }
    }
}