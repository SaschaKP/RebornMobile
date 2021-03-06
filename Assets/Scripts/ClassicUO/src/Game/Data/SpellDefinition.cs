

using System;
using System.Collections.Generic;
using System.Text;

using ClassicUO.Game.Managers;
using ClassicUO.Utility;

namespace ClassicUO.Game.Data
{
    internal class SpellDefinition : IEquatable<SpellDefinition>
    {
        public static SpellDefinition EmptySpell = new SpellDefinition("", 0, 0, "", 0, 0, 0);

        internal static Dictionary<string, SpellDefinition> WordToTargettype = new Dictionary<string, SpellDefinition>();


        public SpellDefinition(string name, int index, int gumpIconID, int gumpSmallIconID, string powerwords, int manacost, int minskill, int tithingcost, TargetType target, params Reagents[] regs)
        {
            Name = name;
            ID = index;
            GumpIconID = gumpIconID;
            GumpIconSmallID = gumpSmallIconID;
            Regs = regs;
            ManaCost = manacost;
            MinSkill = minskill;
            PowerWords = powerwords;
            TithingCost = tithingcost;
            TargetType = target;
            AddToWatchedSpell();
        }

        public SpellDefinition(string name, int index, int gumpIconID, string powerwords, int manacost, int minskill, TargetType target, params Reagents[] regs)
        {
            Name = name;
            ID = index;
            GumpIconID = gumpIconID;
            GumpIconSmallID = gumpIconID;
            Regs = regs;
            ManaCost = manacost;
            MinSkill = minskill;
            PowerWords = powerwords;
            TithingCost = 0;
            TargetType = target;
            AddToWatchedSpell();
        }

        public SpellDefinition(string name, int index, int gumpIconID, string powerwords, TargetType target, params Reagents[] regs)
        {
            Name = name;
            ID = index;
            GumpIconID = gumpIconID;
            GumpIconSmallID = gumpIconID - 0x1298;
            Regs = regs;
            ManaCost = 0;
            MinSkill = 0;
            TithingCost = 0;
            PowerWords = powerwords;
            TargetType = target;
            AddToWatchedSpell();
        }

        private void AddToWatchedSpell()
        {
            if (!string.IsNullOrEmpty(PowerWords))
                WordToTargettype[PowerWords] = this;
            else if (!string.IsNullOrEmpty(Name))
                WordToTargettype[Name] = this;
        }

        public readonly string Name;
        public readonly int ID;
        public readonly int GumpIconID;
        public readonly int GumpIconSmallID;
        public readonly Reagents[] Regs;
        public readonly string PowerWords;
        public readonly int ManaCost;
        public readonly int MinSkill;
        public readonly int TithingCost;
        public readonly TargetType TargetType;


        public string CreateReagentListString(string separator)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < Regs.Length; i++)
            {
                switch (Regs[i])
                {
                    // britanian reagents
                    case Reagents.BlackPearl:
                        sb.Append("Black Pearl");

                        break;

                    case Reagents.Bloodmoss:
                        sb.Append("Bloodmoss");

                        break;

                    case Reagents.Garlic:
                        sb.Append("Garlic");

                        break;

                    case Reagents.Ginseng:
                        sb.Append("Ginseng");

                        break;

                    case Reagents.MandrakeRoot:
                        sb.Append("Mandrake Root");

                        break;

                    case Reagents.Nightshade:
                        sb.Append("Nightshade");

                        break;

                    case Reagents.SulfurousAsh:
                        sb.Append("Sulfurous Ash");

                        break;

                    case Reagents.SpidersSilk:
                        sb.Append("Spiders Silk");

                        break;

                    // pagan reagents
                    case Reagents.BatWing:
                        sb.Append("Bat Wing");

                        break;

                    case Reagents.GraveDust:
                        sb.Append("Grave Dust");

                        break;

                    case Reagents.DaemonBlood:
                        sb.Append("Daemon Blood");

                        break;

                    case Reagents.NoxCrystal:
                        sb.Append("Nox Crystal");

                        break;

                    case Reagents.PigIron:
                        sb.Append("Pig Iron");

                        break;

                    default:

                        if (Regs[i] < Reagents.None)
                            sb.Append(StringHelper.AddSpaceBeforeCapital(Regs[i].ToString()));

                        break;
                }

                if (i < Regs.Length - 1)
                    sb.Append(separator);
            }

            return sb.ToString();
        }

        public bool Equals(SpellDefinition other)
        {
            return ID.Equals(other.ID);
        }

        public static SpellDefinition FullIndexGetSpell(int fullidx)
        {
            if (fullidx < 1 || fullidx > 799)
                return EmptySpell;

            if (fullidx < 100)
                return SpellsMagery.GetSpell(fullidx);

            if (fullidx < 200)
                return SpellsNecromancy.GetSpell(fullidx % 100);

            if (fullidx < 300)
                return SpellsChivalry.GetSpell(fullidx % 100);

            if (fullidx < 500)
                return SpellsBushido.GetSpell(fullidx % 100);

            if (fullidx < 600)
                return SpellsNinjitsu.GetSpell(fullidx % 100);

            if (fullidx < 678)
                return SpellsSpellweaving.GetSpell(fullidx % 100);

            if (fullidx < 700)
                return SpellsMysticism.GetSpell((fullidx - 77) % 100);

            return SpellsMastery.GetSpell(fullidx % 100);
        }

        public static void FullIndexSetModifySpell(int fullidx, int id, int iconid, int smalliconid, int minskill, int manacost, int tithing, string name, string words, TargetType target, params Reagents[] regs)
        {
            if (fullidx < 1 || fullidx > 799)
                return;

            SpellDefinition sd = FullIndexGetSpell(fullidx);

            if (sd.ID == fullidx) //we are not using an emptyspell spelldefinition
            {
                if (iconid == 0)
                    iconid = sd.GumpIconID;

                if (smalliconid == 0)
                    smalliconid = sd.GumpIconSmallID;

                if (tithing == 0)
                    tithing = sd.TithingCost;

                if (manacost == 0)
                    manacost = sd.ManaCost;

                if (minskill == 0)
                    minskill = sd.MinSkill;

                if (!string.IsNullOrEmpty(sd.PowerWords) && sd.PowerWords != words) WordToTargettype.Remove(sd.PowerWords);
                if (!string.IsNullOrEmpty(sd.Name) && sd.Name != name) WordToTargettype.Remove(sd.Name);
            }

            sd = new SpellDefinition(name, fullidx, iconid, smalliconid, words, manacost, minskill, tithing, target, regs);

            if (fullidx < 100)
                SpellsMagery.SetSpell(id, in sd);
            else if (fullidx < 200)
                SpellsNecromancy.SetSpell(id, in sd);
            else if (fullidx < 300)
                SpellsChivalry.SetSpell(id, in sd);
            else if (fullidx < 500)
                SpellsBushido.SetSpell(id, in sd);
            else if (fullidx < 600)
                SpellsNinjitsu.SetSpell(id, in sd);
            else if (fullidx < 678)
                SpellsSpellweaving.SetSpell(id, in sd);
            else if (fullidx < 700)
                SpellsMysticism.SetSpell(id - 77, in sd);
            else
                SpellsMastery.SetSpell(id, in sd);
        }
    }
}