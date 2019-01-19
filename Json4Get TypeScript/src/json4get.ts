import { isNullOrWhiteSpace } from "./helpers";
import { JsonCompressor } from "./json-compressor";
import { Characters } from "./characters";

/**
 * Special converter which takes json and reformats it to better work in a 
 * http-get call for the url. 
 * Basically it converts very common characters like {, } and " into simpler characters
 * ...and back.
 */
export class Json4Get {
  public static encode(original: string): string {
    //#region First, do basic validity checking
    if (isNullOrWhiteSpace(original)) return original;
    // VerifyStartingCharIsValid(ref original, Characters.JsonStartMarkers);
    //#endregion

    original = new JsonCompressor().compress(original);
    let builder = "";
    var outsideOfQuotes = true;
    var previousCharacter = '';
    var openCloseCount = 0; // keep track of open/close cases, as in the end we should be back at zero
    for (var ci = 0; ci < original.length; ci++) {
      const currentChar = original.charAt(ci);
      // #region Case 1: We are not inside a value "value" yet, so check for {, } and "
      if (outsideOfQuotes) {
        var index = Characters.Specials.indexOf(currentChar);
        if (index != -1) {
          builder += Characters.Replacements[index];
          openCloseCount += Characters.OpenCounters[index];
          if (currentChar == Characters.QuoteOriginal)
            outsideOfQuotes = false;
        }
        else builder += currentChar;
      }
      // #endregion

      // #region Case 2: we are inside a quoted "value", so don't replace {} but escape single '
      else {
        if (currentChar == Characters.QuoteEncoded || currentChar == Characters.SpaceReplacement)
          builder += Characters.EscapePrefix + currentChar;
        else if (currentChar == Characters.Space)
          builder += Characters.SpaceReplacement;
        else if (currentChar == Characters.QuoteOriginal && previousCharacter != Characters.EscapePrefix) {
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

  public static decode(original: string): string {
    // #region First, do basic validity checking
    if (isNullOrWhiteSpace(original)) return original;
    original = original.trim();
    // VerifyStartingCharIsValid(ref original, Characters.Json4GetStartMarkers);
    // #endregion

    var builder = '';
    var outsideOfQuotes = true;
    var previousCharacters = '';

    for (var ci = 0; ci < original.length; ci++) {
      const currentChar = original.charAt(ci);
      // #region Looking at chars outside a "value" so replace (, ) and '

      if (outsideOfQuotes) {
        var index = Characters.Replacements.indexOf(currentChar);
        if (index == -1)
          builder += currentChar;
        else {
          builder += Characters.Specials[index];
          if (currentChar == Characters.QuoteEncoded)
            outsideOfQuotes = false;
        }
      }
      // #endregion

      // #region Looking at chars inside a "value" so leave () alone
      else switch (currentChar) {
        case Characters.SpaceReplacement:
          if (previousCharacters == Characters.EscapePrefix)
            builder = builder.substr(0, builder.length - 1) + currentChar;
          else
            builder += Characters.Space;
          break;
        case Characters.QuoteEncoded:
          if (previousCharacters == Characters.EscapePrefix)
            builder = builder.substr(0, builder.length - 1) + currentChar;
          else {
            builder += Characters.QuoteOriginal;
            outsideOfQuotes = true;
          }
          break;
        default:
          builder += currentChar;
          break;
      }
      // #endregion

      // remember the char for next checks...
      previousCharacters = currentChar;
    }
    var result = builder;
    return new JsonCompressor().decompress(result);
  }
}