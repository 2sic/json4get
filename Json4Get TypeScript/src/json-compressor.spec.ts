import { JsonCompressor } from "./json-compressor";

describe("Json compressor should trim bad json", () => {
  const testJson = "  { }";
  var comp = new JsonCompressor();
  it("Should be trimmed", () => {
    const compressed = comp.Compress(testJson);
    expect(compressed).toBe("{}");
  });

  it("should trim a true", () => {
    expect(comp.Compress('{ true }')).toBe('{t}');
  });

  it("should compress true, false null", () => {
    expect(comp.Compress('{ "Is": true, "Not": false, "Nothing": null}')).toBe('{"Is":t,"Not":f,"Nothing":n}');
  });
});
