

namespace ClassicUO.Game.Data
{
    enum ConsolePrompt
    {
        None,
        ASCII,
        Unicode
    }

    internal struct PromptData
    {
        public ConsolePrompt Prompt;
        public byte[] Data;
    }
}