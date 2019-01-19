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

  public Compress(json: string): string
  {
    for(let i = 0; i < this.Compressors.length; i++) {
      let c = this.Compressors[i];
      json = json.replace(c.rex, c.replace);
    }
    return json;
  }






}

function BuildOutsideOfWs(searching: string): RegExp {
  const filter = '(?=((\\[\\"]|[^\\"])*"(\\[\\"]|[^\\"])*")*(\\[\\"]|[^\\"])*$)';
  return new RegExp(searching + filter, "gm");
}
