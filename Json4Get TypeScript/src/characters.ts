export class Characters {
  public static QuoteOriginal = '"';
  public static QuoteEncoded = '\'';
  public static Space = ' ';
  public static SpaceReplacement = '_';

  public static Specials = ":,{}[]\"";
  public static Replacements = "!*()LJ'";
  public static OpenCounters = [ 0, 0, 1, -1, 1, -1, 1 ];
  public static EscapePrefix = '\\';
  public static JsonStartMarkers = "{[\"0123456789ntf";

  public static Json4GetStartMarkers = "(['0123456789ntf";
}