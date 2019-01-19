import { isNullOrWhiteSpace } from "./helpers";
import { JsonCompressor } from "./json-compressor";
import { Characters } from "./characters";

export class Json4Get{
  public static encode(original: string): string
  {
      //#region First, do basic validity checking
      if (isNullOrWhiteSpace(original)) return original;
      // VerifyStartingCharIsValid(ref original, Characters.JsonStartMarkers);
      //#endregion

      original = new JsonCompressor().compress(original);
      let builder = "";//new StringBuilder();
      var outsideOfQuotes = true;
      var previousCharacter = '';//default(char);
      var openCloseCount = 0; // keep track of open/close cases, as in the end we should be back at zero
      for(var ci =0; ci < origin.length; ci++)
      // foreach (var currentChar in original)
      {
        const currentChar = origin.charAt(ci);
          // #region Case 1: We are not inside a value "value" yet, so check for {, } and "
          if (outsideOfQuotes)
          {
              var index = Characters.Specials.indexOf(currentChar);
              if (index != -1)
              {
                builder += Characters.Replacements[index];
                openCloseCount += Characters.OpenCounters[index];
                if (currentChar == Characters.QuoteOriginal)
                  outsideOfQuotes = false;
              }
              else builder += currentChar;
          }
          // #endregion

          // #region Case 2: we are inside a quoted "value", so don't replace {} but escape single '
          else
          {
              if (currentChar == Characters.QuoteEncoded || currentChar == Characters.SpaceReplacement)
                  builder += Characters.EscapePrefix + currentChar;
              else if (currentChar == Characters.Space)
                  builder += Characters.SpaceReplacement;
              else if (currentChar == Characters.QuoteOriginal && previousCharacter != Characters.EscapePrefix)
              {
                  builder += Characters.QuoteEncoded;
                  openCloseCount--;
                  outsideOfQuotes = true;
              }
              else
                  builder += currentChar;
          }
          // #endregion

          // remember this char, in case the next character-check needs to know it
          previousCharacter = currentChar;
      }

      // #region final error-checking
      if (openCloseCount != 0)
          throw `Cannot convert json4get, total opening / closing brackets and quotes don't match, got ${openCloseCount}`;
      // #endregion

      return builder;
  }
}