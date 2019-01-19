import { JsonCompressor } from "../json-compressor";

describe("Json compressor should trim bad json", () => {
  const testJson = "  { }";
  const comp = new JsonCompressor();
  it("Should be trimmed", () => {
    const compressed = comp.compress(testJson);
    expect(compressed).toBe("{}");
  });

  it("should trim a true", () => {
    expect(comp.compress("{ true }")).toBe("{t}");
  });

  const testTfn = '{"Is":true,"Not":false,"Nothing":null}';
  const compTfn = comp.compress(testTfn);
  it("should compress true, false null", () => {
    expect(compTfn).toBe('{"Is":t,"Not":f,"Nothing":n}');
  });

  const uncompTfn = comp.decompress(compTfn);
  it("should compress true, false null", () => {
    expect(uncompTfn).toBe(testTfn);
  });

});
