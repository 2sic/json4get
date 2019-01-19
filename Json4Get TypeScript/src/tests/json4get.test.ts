import { Json4Get } from "../json4get";


describe("Json4Get", () => {
  const simple = JSON.stringify({Name: "Daniel"});
  const result = "('Name'!'Daniel')";
  const inGet = Json4Get.encode(simple);
  it("should match", () => expect(inGet).toBe(result));

  it("should fail", () => expect(inGet).toBe(result));
});
