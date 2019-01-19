import { Json4Get } from "../json4get";


describe("Json4Get", () => {
  const simple = JSON.stringify({Name: "Daniel"});
  const result = "('Name'!'Daniel')";
  const inGet = Json4Get.encode(simple);
  it("should match", () => expect(inGet).toBe(result));

  var decod = Json4Get.decode(inGet);
  it("should fail", () => expect(decod).toBe(simple));
});
