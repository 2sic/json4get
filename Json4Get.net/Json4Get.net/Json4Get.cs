using System;
using System.Linq;
using System.Text;

namespace ToSic.Json4Get
{
    /// <summary>
    /// Special converter which takes json and reformats it to better work in a 
    /// http-get call for the url. 
    /// Basically it converts very common characters like {, } and " into simpler characters
    /// ...and back.
    /// </summary>
    public static class Json4Get
    {
        /// <summary>
        /// Convert a JSON into a nicer GET capable format
        /// </summary>
        /// <param name="original"></param>
        /// <param name="enableQuoteShrinking"></param>
        /// <remarks>
        /// in will test for various bad inputs, but in general you should be sure to only throw real
        /// JSON at it.
        /// </remarks>
        /// <returns></returns>
        public static string Encode(string original, bool enableQuoteShrinking = false) => new Encoder(original, enableQuoteShrinking).Encode();

        /// <summary>
        /// Decode Json4Get to JSON
        /// </summary>
        /// <param name="original"></param>
        /// <remarks>
        /// in will test for various bad inputs, but in general you should be sure to only throw real
        /// JSON4GET at it.
        /// </remarks>
        /// <returns></returns>
        public static string Decode(string original) => new Decoder(original).Decode();
    }

    internal class Encoder: EncDecBase
    {
        internal Encoder(string original, bool enableQuoteShrinking = false) : base(original)
        {
            EnableQuoteShrinking = enableQuoteShrinking;
        }
        internal readonly bool EnableQuoteShrinking;


        internal StringBuilder ValueBuilder = new StringBuilder();
        internal bool OutsideOfQuotes = true;
        internal char PrevCharacter = default(char);
        internal int OpenCloseCount; // keep track of open/close cases, as in the end we should be back at zero

        public string Encode()
        {
            // First, do basic validity checking
            if (string.IsNullOrWhiteSpace(Original)) return Original;
            VerifyStartingCharIsValid(Characters.JsonStartMarkers);
            Original = JsonCompressor.Compress(Original);

            // not process each character
            foreach (var currentChar in Original)
            {
                if (OutsideOfQuotes)
                    ProcessOutsideOfQuote(currentChar);
                else
                    ProcessInsideValue(currentChar);

                // remember this char, in case the next character-check needs to know it
                PrevCharacter = currentChar;
            }


            // final error-checking
            if (OpenCloseCount != 0)
                throw new Exception($"Cannot convert json4get, total opening / closing brackets and quotes don't match, got {OpenCloseCount}");

            return Builder.ToString();
        }

        private void ProcessInsideValue(char currentChar)
        {
            if (currentChar == Characters.QuoteOriginal && PrevCharacter != Characters.EscapePrefix)
            {
                var value = ValueBuilder.ToString();
                var skipQuotes = EnableQuoteShrinking && !NeedsQuotes(value);

                if (!skipQuotes)
                    Builder.Append(Characters.QuoteEncoded);
                Builder.Append(EncodeValue(value));
                if (!skipQuotes)
                    Builder.Append(Characters.QuoteEncoded);

                // Reset value state
                OpenCloseCount--;
                ValueBuilder.Clear();
                OutsideOfQuotes = true;
            }
            else
                ValueBuilder.Append(currentChar);
        }


        private void ProcessOutsideOfQuote(char currentChar)
        {
            var index = Characters.Specials.IndexOf(currentChar);
            if (index != -1)
            {
                OpenCloseCount += Characters.OpenCounters[index];
                if (currentChar == Characters.QuoteOriginal)
                    OutsideOfQuotes = false;
                else
                    Builder.Append(Characters.Replacements[index]);
            }
            else Builder.Append(currentChar);
        }

        private static readonly char[] UnsafeChars = "!'*LJ}".ToCharArray();
        internal static bool NeedsQuotes(string value)
        {
            // empty requires quotes
            if (value.Length == 0) return true;
            // single char which is reserved requires quotes
            if (value.Length == 1 && JsonCompressor.StructureAbbreviations.Contains(value[0])) return true;
            // check for unsafe characters
            return value.Any(character => UnsafeChars.Contains(character));
        }
    }


    internal class Decoder: EncDecBase
    {
        internal Decoder(string original) : base(original) { }

        internal readonly StringBuilder Fragment = new StringBuilder();

        internal bool OutsideOfQuotes = true;
        internal char PrevChar = default(char);
        internal string Decode()
        {
            // First, do basic validity checking
            if (string.IsNullOrWhiteSpace(Original)) return Original;
            Original = Original.Trim();
            VerifyStartingCharIsValid(Characters.Json4GetStartMarkers);

            foreach (var currentChar in Original)
            {
                if (OutsideOfQuotes)
                    ParseOutsideOfQuote(currentChar);
                else
                    ParseInsideQuotes(currentChar);

                PrevChar = currentChar; // remember the char for next checks...
            }
            FlushFragment();
            var result = Builder.ToString();
            return JsonCompressor.Decompress(result);
        }

        private void FlushFragment()
        {
            Builder.Append(Fragment);
            Fragment.Clear();
        }

        private void ParseInsideQuotes(char currentChar)
        {
            switch (currentChar)
            {
                case Characters.QuoteEncoded:
                    if (PrevChar == Characters.EscapePrefix)
                        Fragment.ReplaceLast(currentChar);
                    else
                    {
                        // WIP here
                        // Working on later auto-detecting if we add quotes around the decrypted values
                        var val = DecodeValue(Fragment.ToString());
                        Builder.Append($"{Characters.QuoteOriginal}{val}{Characters.QuoteOriginal}");
                        Fragment.Clear();
                        OutsideOfQuotes = true;
                    }
                    break;
                default:
                    Fragment.Append(currentChar);
                    break;
            }
        }


        private string DecodeFragment(string fragment, bool forceQuotes)
        {
            // empty value
            if (string.IsNullOrEmpty(fragment))
                return !forceQuotes ? fragment : new string(Characters.QuoteOriginal, 2);

            // one of the n/t/f values = null, true, false
            if (fragment.Length == 1 && JsonCompressor.StructureAbbreviations.Trim().Contains(fragment[0]))
                return !forceQuotes ? fragment : $"{Characters.QuoteOriginal}{fragment}{Characters.QuoteEncoded}";

            var nextCharIsEscaped = false;
            var fragBuilder = new StringBuilder();
            foreach (var currentChar in fragment)
            {
                if (nextCharIsEscaped)
                {
                    fragBuilder.Append(currentChar);
                    nextCharIsEscaped = false;
                    continue;
                }

                switch (currentChar)
                {
                    case Characters.EscapePrefix:
                        nextCharIsEscaped = true;
                        break;
                    case Characters.SpaceReplacement:
                        fragBuilder.Append(Characters.Space);
                        break;
                    default:
                        fragBuilder.Append(currentChar);
                        break;
                }
            }

            // return result
            return forceQuotes 
                ? $"{Characters.QuoteOriginal}{fragBuilder}{Characters.QuoteOriginal}" 
                : $"{fragBuilder}";
        }

        /// <summary>
        /// Looking at chars outside a "value" so replace (, ) and '
        /// </summary>
        /// <param name="currentChar"></param>
        private void ParseOutsideOfQuote(char currentChar)
        {
            var index = Characters.Replacements.IndexOf(currentChar);
            if (index == -1)
                Fragment.Append(currentChar);
            else
            {
                if (currentChar != Characters.QuoteEncoded)
                    Fragment.Append(Characters.Specials[index]);
                else
                {
                    FlushFragment();
                    OutsideOfQuotes = false;
                }
            }
        }
    }

    public static class Helpers
    {
        #region Extension Methods for StringBuilder
        public static void ReplaceLast(this StringBuilder builder, char replacement)
        {
            builder.Length--;
            builder.Append(replacement);
        }
        #endregion

    }

    internal class EncDecBase
    {
        protected string Original;
        protected EncDecBase(string original) => Original = original;

        protected StringBuilder Builder = new StringBuilder();

        protected void VerifyStartingCharIsValid(string allowedFirstChars)
        {
            foreach (var character in Original)
            {
                // found allowed / expected character, ok
                if (allowedFirstChars.IndexOf(character) >= 0) return;
                // whitespace, keep testing
                if (char.IsWhiteSpace(character)) continue;
                // none of the above, throw
                throw new Exception("Cannot encode json4get - first character does not seem to be a json starter");
            }
        }


        protected Tuple<string, string>[] ValueEncodes = {
            new Tuple<string, string>($"{Characters.QuoteEncoded}",
                $"{Characters.EscapePrefix}{Characters.QuoteEncoded}"),
            new Tuple<string, string>($"{Characters.SpaceReplacement}",
                $"{Characters.EscapePrefix}{Characters.SpaceReplacement}"),
            new Tuple<string, string>(Characters.Space.ToString(), 
                Characters.SpaceReplacement.ToString()),
            new Tuple<string, string>($"{Characters.EscapePrefix}{Characters.QuoteOriginal}",
                $"{Characters.QuoteOriginal}"),
        };

        protected string EncodeValue(string value) 
            => ValueEncodes.Aggregate(value, (current, t) => current.Replace(t.Item1, t.Item2));

        protected string DecodeValue(string value) 
            => ValueEncodes.Reverse().Aggregate(value, (current, t) => current.Replace(t.Item2, t.Item1));
    }

}
