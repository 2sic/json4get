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
        /// <remarks>
        /// in will test for various bad inputs, but in general you should be sure to only throw real
        /// JSON at it.
        /// </remarks>
        /// <returns></returns>
        public static string Encode(string original) => new Encoder(original).Encode();

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

    internal class Encoder
    {
        internal string Original;
        internal Encoder(string original) => Original = original;

        internal StringBuilder Builder = new StringBuilder();
        internal StringBuilder ValueBuilder = new StringBuilder();
        internal bool OutsideOfQuotes = true;
        internal char PrevCharacter = default(char);
        internal int openCloseCount = 0; // keep track of open/close cases, as in the end we should be back at zero

        public string Encode()
        {
            // First, do basic validity checking
            if (string.IsNullOrWhiteSpace(Original)) return Original;
            Helpers.VerifyStartingCharIsValid(ref Original, Characters.JsonStartMarkers);
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
            if (openCloseCount != 0)
                throw new Exception($"Cannot convert json4get, total opening / closing brackets and quotes don't match, got {openCloseCount}");

            return Builder.ToString();
        }

        private void ProcessInsideValue(char currentChar)
        {
            if (currentChar == Characters.QuoteEncoded || currentChar == Characters.SpaceReplacement)
                ValueBuilder.Append(Characters.EscapePrefix).Append(currentChar);
            else if (currentChar == Characters.Space)
                ValueBuilder.Append(Characters.SpaceReplacement);
            // Case leaving the quoted value
            else if (currentChar == Characters.QuoteOriginal && PrevCharacter != Characters.EscapePrefix)
            {
                var value = ValueBuilder.ToString();
                var needsQuotes = true; // NeedsQuotes(value);

                if (needsQuotes)
                    Builder.Append(Characters.QuoteEncoded);
                Builder.Append(value);
                if (needsQuotes)
                    Builder.Append(Characters.QuoteEncoded);

                // Reset value state
                openCloseCount--;
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
                openCloseCount += Characters.OpenCounters[index];
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


    internal class Decoder
    {
        internal string Original;
        internal Decoder(string original) => Original = original;

        internal readonly StringBuilder Builder = new StringBuilder();
        internal readonly StringBuilder Fragment = new StringBuilder();

        internal bool OutsideOfQuotes = true;
        internal char PrevChar = default(char);
        internal string Decode()
        {
            #region First, do basic validity checking
            if (string.IsNullOrWhiteSpace(Original)) return Original;
            Original = Original.Trim();
            Helpers.VerifyStartingCharIsValid(ref Original, Characters.Json4GetStartMarkers);
            #endregion

            foreach (var currentChar in Original)
            {
                if (OutsideOfQuotes)
                    ParseOutsideOfQuote(currentChar);
                else
                    ParseInsideQuotes(currentChar);

                PrevChar = currentChar; // remember the char for next checks...
            }
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
                case Characters.SpaceReplacement:
                    if (PrevChar == Characters.EscapePrefix)
                        Fragment.ReplaceLast(currentChar);
                    else
                        Fragment.Append(Characters.Space);
                    break;
                case Characters.QuoteEncoded:
                    if (PrevChar == Characters.EscapePrefix)
                        Fragment.ReplaceLast(currentChar);
                    else
                    {
                        Fragment.Append(Characters.QuoteOriginal);
                        FlushFragment();
                        OutsideOfQuotes = true;
                    }
                    break;
                default:
                    Fragment.Append(currentChar);
                    break;
            }
        }

        /// <summary>
        /// Looking at chars outside a "value" so replace (, ) and '
        /// </summary>
        /// <param name="currentChar"></param>
        private void ParseOutsideOfQuote(char currentChar)
        {
            var index = Characters.Replacements.IndexOf(currentChar);
            if (index == -1)
                Builder.Append(currentChar);
            else
            {
                Builder.Append(Characters.Specials[index]);
                if (currentChar == Characters.QuoteEncoded)
                    OutsideOfQuotes = false;
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

        internal static void VerifyStartingCharIsValid(ref string original, string allowedFirstChars)
        {
            foreach (var character in original)
            {
                // found allowed / expected character, ok
                if (allowedFirstChars.IndexOf(character) >= 0) return;

                // whitespace, keep testing
                if (Char.IsWhiteSpace(character)) continue;

                // none of the above, throw
                throw new Exception("Cannot encode json4get - first character does not seem to be a json starter");
            }
        }
    }

}
