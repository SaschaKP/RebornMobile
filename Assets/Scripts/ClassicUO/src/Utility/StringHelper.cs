

using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using SDL2;

namespace ClassicUO.Utility
{
    internal static class StringHelper
    {
        private static readonly char[] _dots = {'.', ',', ';', '!'};
        private static readonly StringBuilder _sb = new StringBuilder();

        public static string CapitalizeFirstCharacter(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            if (str.Length == 1)
                return char.ToUpper(str[0]).ToString();

            return char.ToUpper(str[0]) + str.Substring(1);
        }


        public static string CapitalizeAllWords(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            if (str.Length == 1)
                return char.ToUpper(str[0]).ToString();

            _sb.Clear();

            bool capitalizeNext = true;

            for (int i = 0; i < str.Length; i++)
            {
                _sb.Append(capitalizeNext ? char.ToUpper(str[i]) : str[i]);
                if (!char.IsWhiteSpace(str[i]))
                    capitalizeNext = i + 1 < str.Length && char.IsWhiteSpace(str[i + 1]);
            }

            return _sb.ToString();
        }

        public static string CapitalizeWordsByLimitator(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            if (str.Length == 1)
                return char.ToUpper(str[0]).ToString();

            _sb.Clear();

            bool capitalizeNext = true;

            for (int i = 0; i < str.Length; i++)
            {
                _sb.Append(capitalizeNext ? char.ToUpper(str[i]) : str[i]);
                capitalizeNext = false;

                for (int j = 0; j < _dots.Length; j++)
                {
                    if (str[i] == _dots[j])
                    {
                        capitalizeNext = true;

                        break;
                    }
                }
            }

            return _sb.ToString();
        }

        public static unsafe string ReadUTF8(byte* data)
        {
            byte* ptr = data;

            while (*ptr != 0)
                ptr++;

            return Encoding.UTF8.GetString(data, (int) (ptr - data));
        }

        [MethodImpl(256)]
        public static bool IsSafeChar(int c)
        {
            return c >= 0x20 && c < 0xFFFE;
        }

        public static void AddSpaceBeforeCapital(string[] str, bool checkAcronyms = true)
        {
            for (int i = 0; i < str.Length; i++) str[i] = AddSpaceBeforeCapital(str[i], checkAcronyms);
        }

        public static string AddSpaceBeforeCapital(string str, bool checkAcronyms = true)
        {
            if (string.IsNullOrWhiteSpace(str))
                return "";

            _sb.Clear();
            _sb.Append(str[0]);

            for (int i = 1, len = str.Length - 1; i <= len; i++)
            {
                if (char.IsUpper(str[i]))
                {
                    if (str[i - 1] != ' ' && !char.IsUpper(str[i - 1]) ||
                        checkAcronyms && char.IsUpper(str[i - 1]) && i < len && !char.IsUpper(str[i + 1]))
                        _sb.Append(' ');
                }

                _sb.Append(str[i]);
            }

            return _sb.ToString();
        }

        public static string RemoveUpperLowerChars(string str, bool removelower = true)
        {
            if (string.IsNullOrWhiteSpace(str))
                return "";

            _sb.Clear();

            for (int i = 0; i < str.Length; i++)
            {
                if (char.IsUpper(str[i]) == removelower || str[i] == ' ')
                    _sb.Append(str[i]);
            }

            return _sb.ToString();
        }

        public static string IntToAbbreviatedString(int num)
        {
            if (num > 999999)
            {
                return string.Format("{0}M+", num / 1000000);
            }
            else if (num > 999)
            {
                return string.Format("{0}K+", num / 1000);
            }
            else
            {
                return num.ToString();
            }
        }

        public static string GetClipboardText(bool multiline)
        {
            if (SDL.SDL_HasClipboardText() != SDL.SDL_bool.SDL_FALSE)
            {
                string s = multiline ? SDL.SDL_GetClipboardText() : (SDL.SDL_GetClipboardText()?.Replace('\n', ' ') ?? null );
                if(!string.IsNullOrEmpty(s))
                {
                    if (s.IndexOf('\t') >= 0)
                        return s.Replace("\t", "   ");
                    return s;
                }
            }
            return null;
        }

        private static readonly Dictionary<char, string> Replacements = new Dictionary<char, string>();
        /// <summary>Returns the specified string with characters not representable in ASCII codepage 437 converted to a suitable representative equivalent.</summary>
        /// <param name="s">A string.</param>
        /// <returns>The supplied string, with smart quotes, fractions, accents and punctuation marks 'normalized' to ASCII equivalents.</returns>
        public static string Asciify(string s)
        {
            _sb.Clear();
            string r;
            foreach(char c in s)
            {
                if (Replacements.TryGetValue(c, out r))
                    _sb.Append(r);
                else
                    _sb.Append(c);
            }
            return _sb.ToString();
        }

        private static string Asciify(char x)
        {
            return Replacements.ContainsKey(x) ? (Replacements[x]) : (x.ToString());
        }

        static StringHelper()
        {
            Replacements['’'] = "'";
            Replacements['–'] = "-";
            Replacements['‘'] = "'";
            Replacements['”'] = "\"";
            Replacements['“'] = "\"";
            Replacements['…'] = "...";
            //Replacements['£'] = "GBP";
            Replacements['•'] = "*";
            Replacements[' '] = " ";
            //Replacements['é'] = "e";
            //Replacements['ï'] = "i";
            Replacements['´'] = "'";
            Replacements['—'] = "-";
            Replacements['·'] = "*";
            Replacements['„'] = "\"";
            //Replacements['€'] = "EUR";
            //Replacements['®'] = "(R)";
            //Replacements['¹'] = "(1)";
            //Replacements['«'] = "\"";
            /*Replacements['è'] = "e";
            Replacements['á'] = "a";
            Replacements['™'] = "TM"; 
            Replacements['»'] = "\""; 
            Replacements['ç'] = "c";
            Replacements['½'] = "1/2";
            Replacements['­'] = "-";
            //Replacements['°'] = " degree";
            /*Replacements['ä'] = "a"
            Replacements['É'] = "E";
            Replacements['‚'] = ",";
            Replacements['ü'] = "u";
            Replacements['í'] = "i";
            Replacements['ë'] = "e";
            Replacements['ö'] = "o";
            Replacements['à'] = "a";
            Replacements['¬'] = " ";
            /*Replacements['ó'] = "o";
            Replacements['â'] = "a";
            Replacements['ñ'] = "n";
            Replacements['ô'] = "o";
            Replacements['¨'] = "";
            //Replacements['å'] = "a";
            //Replacements['ã'] = "a";
            Replacements['ˆ'] = "";
            /*Replacements['©'] = "(c)";
            Replacements['Ä'] = "A";
            Replacements['Ï'] = "I";
            Replacements['ò'] = "o";
            Replacements['ê'] = "e";
            Replacements['î'] = "i";
            Replacements['Ü'] = "U";
            Replacements['Á'] = "A";
            Replacements['ß'] = "ss";
            Replacements['¾'] = "3/4";
            Replacements['È'] = "E";
            Replacements['¼'] = "1/4";
            Replacements['†'] = "+";
            Replacements['³'] = "'";
            Replacements['²'] = "'";
            Replacements['Ø'] = "O";*/
            Replacements['¸'] = ",";
            /*Replacements['Ë'] = "E";
            Replacements['ú'] = "u";
            Replacements['Ö'] = "O";
            Replacements['û'] = "u";
            Replacements['Ú'] = "U";
            Replacements['Œ'] = "Oe";
            Replacements['º'] = "?";
            Replacements['‰'] = "0/00";
            Replacements['Å'] = "A";
            Replacements['ø'] = "o";*/
            Replacements['˜'] = "~";
            /*Replacements['æ'] = "ae";
            Replacements['ù'] = "u";*/
            Replacements['‹'] = "<";
            //Replacements['±'] = "+/-";
        }
    }
}