

namespace ClassicUO.Game.Data
{
    enum Lock : byte
    {
        Up = 0,
        Down = 1,
        Locked = 2
    }

    internal sealed class Skill
    {
        public Skill(string name, int index, bool click)
        {
            Name = name;
            Index = index;
            IsClickable = click;
        }

        public Lock Lock { get; internal set; }

        public ushort ValueFixed { get; internal set; }

        public ushort BaseFixed { get; internal set; }

        public ushort CapFixed { get; internal set; }

        public float Value => ValueFixed / 10.0f;

        public float Base => BaseFixed / 10.0f;

        public float Cap => CapFixed / 10.0f;

        public bool IsClickable { get; }

        public string Name { get; }

        public int Index { get; }

        public override string ToString()
        {
            return $"Name: {Name} - {Value:F1}";
        }
    }
}