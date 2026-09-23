using System;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEngine;

namespace SimpleOfflineTTS
{
    public sealed class SupertonicCharTokenizer
    {
        private readonly int[] _unicodeIndexer;

        private readonly int _modelMaxLength;

        // Unity/Mono regex does not reliably support surrogate-pair ranges (emoji ranges)
        // inside a character class, so we remove emoji/astral symbols via code instead.
        private static readonly Regex DashRegex = new Regex(@"[\u2011\u2013\u2014]", RegexOptions.Compiled);
        private static readonly Regex DoubleQuoteRegex = new Regex(@"[\u201C\u201D]", RegexOptions.Compiled);
        private static readonly Regex SingleQuoteRegex = new Regex(@"[\u0060\u00B4\u2018\u2019]", RegexOptions.Compiled);
        private static readonly Regex BracketsHashArrowsRegex = new Regex(@"[\[\]\|#→←]", RegexOptions.Compiled);
        private static readonly Regex MultiWhitespaceRegex = new Regex(@"\s+", RegexOptions.Compiled);

        public int ModelMaxLength => _modelMaxLength;

        public SupertonicCharTokenizer(int[] unicodeIndexer, int modelMaxLength = 1000)
        {
            if (unicodeIndexer == null)
            {
                throw new ArgumentNullException(nameof(unicodeIndexer));
            }

            _unicodeIndexer = unicodeIndexer;
            _modelMaxLength = modelMaxLength;
        }

        public static SupertonicCharTokenizer LoadFromJson(TextAsset unicodeIndexerJson, int modelMaxLength = 1000)
        {
            if (unicodeIndexerJson == null)
            {
                throw new ArgumentNullException(nameof(unicodeIndexerJson));
            }

            int[] unicodeIndexer = JsonConvert.DeserializeObject<int[]>(unicodeIndexerJson.text);
            return new SupertonicCharTokenizer(unicodeIndexer, modelMaxLength);
        }

        public string Normalize(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return " ";
            }

            string s = input.Normalize(NormalizationForm.FormKD);

            // Remove emoji / astral symbols / surrogate chars in a Unity-safe way
            s = RemoveSurrogatesAndCommonEmojiBlocks(s);

            s = DashRegex.Replace(s, "-");
            s = DoubleQuoteRegex.Replace(s, "\"");
            s = SingleQuoteRegex.Replace(s, "'");
            s = BracketsHashArrowsRegex.Replace(s, " ");

            s = s.Replace("@", " at ");
            s = s.Replace("e.g.,", "for example, ");
            s = s.Replace("i.e.,", "that is, ");

            s = s.Replace(" ,", ",");
            s = s.Replace(" .", ".");
            s = s.Replace(" !", "!");
            s = s.Replace(" ?", "?");
            s = s.Replace(" ;", ";");
            s = s.Replace(" :", ":");
            s = s.Replace(" '", "'");

            s = MultiWhitespaceRegex.Replace(s, " ");
            s = RemoveCharactersOutsideIndexer(s);

            if (string.IsNullOrEmpty(s))
            {
                return " ";
            }

            return s;
        }

        string RemoveCharactersOutsideIndexer(string s)
        {
            StringBuilder sb = null;

            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                bool keep = !char.IsSurrogate(c) && GetTokenId(c) >= 0;

                if (keep)
                {
                    if (sb != null)
                    {
                        sb.Append(c);
                    }

                    continue;
                }

                if (sb == null)
                {
                    sb = new StringBuilder(s.Length);
                    sb.Append(s, 0, i);
                }
            }

            return sb == null ? s : sb.ToString();
        }

        public void EncodeWithMask(string input, int length, out int[] inputIds, out float[] textMask)
        {
            string normalized = Normalize(input);

            if (length <= 0)
            {
                length = 1;
            }

            if (length > _modelMaxLength)
            {
                length = _modelMaxLength;
            }

            inputIds = new int[length];
            textMask = new float[length];

            int realCount = 0;

            for (int i = 0; i < normalized.Length && realCount < length; i++)
            {
                char c = normalized[i];

                if (char.IsSurrogate(c))
                {
                    continue;
                }

                int id = GetTokenId(c);
                if (id < 0) continue;

                inputIds[realCount] = id;
                textMask[realCount] = 1.0f;

                realCount++;
            }

            for (int i = realCount; i < length; i++)
            {
                inputIds[i] = 0;
                textMask[i] = 0.0f;
            }
        }

        int GetTokenId(char c)
        {
            int code = c;
            if (code < 0 || code >= _unicodeIndexer.Length)
            {
                return -1;
            }

            return _unicodeIndexer[code];
        }

        private static string RemoveSurrogatesAndCommonEmojiBlocks(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            bool hasSurrogate = false;
            for (int i = 0; i < s.Length; i++)
            {
                if (char.IsSurrogate(s[i]))
                {
                    hasSurrogate = true;
                    break;
                }
            }

            if (!hasSurrogate)
            {
                // Still optionally drop BMP emoji-ish blocks like dingbats/symbols if present.
                // If you want the absolute minimum change, you can just return s here.
                return RemoveBmpSymbolBlocks(s);
            }

            StringBuilder sb = new StringBuilder(s.Length);

            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];

                if (char.IsSurrogate(c))
                {
                    continue;
                }

                int code = c;

                // Misc symbols / dingbats often used as emoji-like chars
                if ((code >= 0x2600 && code <= 0x27BF))
                {
                    continue;
                }

                sb.Append(c);
            }

            return sb.ToString();
        }

        private static string RemoveBmpSymbolBlocks(string s)
        {
            // Only removes 0x2600-0x27BF if present; otherwise returns original.
            bool found = false;
            for (int i = 0; i < s.Length; i++)
            {
                int code = s[i];
                if (code >= 0x2600 && code <= 0x27BF)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                return s;
            }

            StringBuilder sb = new StringBuilder(s.Length);

            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                int code = c;

                if (code >= 0x2600 && code <= 0x27BF)
                {
                    continue;
                }

                sb.Append(c);
            }

            return sb.ToString();
        }
    }
}
