export class JsonCompressor {

  public readonly StructureAbbreviations = " tfn";
  public readonly StructureToAbbreviate = [ "\\s+", "true", "false", "null" ];

  private Compressors = this.StructureToAbbreviate.map((s, i) => {
    return {
      replace: this.StructureAbbreviations[i].trim(),
      rex: BuildOutsideOfWs(s),
    };
  });

  private Decompressors = this.StructureToAbbreviate.map((s, i) => {
      return {
        replace: s,
        rex: BuildOutsideOfWs(this.StructureAbbreviations[i]),
      };
  });

  public compress(json: string): string {
    for (const c of this.Compressors) {
      json = json.replace(c.rex, c.replace);
    }
    return json;
  }

  public decompress(compressed: string): string {
    for (const c of this.Decompressors) {
      compressed = compressed.replace(c.rex, c.replace);
    }
    return compressed;
  }

}

function BuildOutsideOfWs(searching: string): RegExp {
  const filter = '(?=((\\[\\"]|[^\\"])*"(\\[\\"]|[^\\"])*")*(\\[\\"]|[^\\"])*$)';
  return new RegExp(searching + filter, "gm");
}
