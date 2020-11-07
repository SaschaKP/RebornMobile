

using System;

using ClassicUO.Configuration;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.UI.Gumps;

namespace ClassicUO.Game.Managers
{
    [Flags]
    internal enum NameOverheadTypeAllowed
    {
        All,
        Mobiles,
        Items,
        Corpses,
        MobilesCorpses = Mobiles | Corpses
    }

    internal static class NameOverHeadManager
    {
        private static NameOverHeadHandlerGump _gump;
        public static NameOverheadTypeAllowed TypeAllowed
        {
            get { return ProfileManager.Current.NameOverheadTypeAllowed; }
            set { ProfileManager.Current.NameOverheadTypeAllowed = value; }
        }

        public static bool IsToggled
        {
            get { return ProfileManager.Current.NameOverheadToggled; }
            set { ProfileManager.Current.NameOverheadToggled = value; }
        }

        public static bool IsAllowed(Entity serial)
        {
            if (serial == null)
                return false;

            if (TypeAllowed == NameOverheadTypeAllowed.All)
                return true;

            if (SerialHelper.IsItem(serial.Serial) && TypeAllowed == NameOverheadTypeAllowed.Items)
                return true;

            if (SerialHelper.IsMobile(serial.Serial) && TypeAllowed.HasFlag(NameOverheadTypeAllowed.Mobiles))
                return true;

            if (TypeAllowed.HasFlag(NameOverheadTypeAllowed.Corpses) && SerialHelper.IsItem(serial.Serial) && World.Items.Get(serial)?.IsCorpse == true)
                return true;

            return false;
        }

        public static void Open()
        {
            if (_gump != null)
                return;

            _gump = new NameOverHeadHandlerGump();
            UIManager.Add(_gump);
        }

        public static void Close()
        {
            if (_gump != null)
            {
                _gump.Dispose();
                _gump = null;
            }
        }

        public static void ToggleOverheads()
        {
            IsToggled = !IsToggled;
        }
    }
}