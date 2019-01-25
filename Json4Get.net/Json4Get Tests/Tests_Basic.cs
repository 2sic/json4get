using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ToSic.Json4Get;

namespace Json4Get_Tests
{
    [TestClass]
    public class Json4Get_Tests
    {
        internal class TestSetBase<T>
        {
            public TestSetBase(T o, string normal, string noQuote = null)
            {
                Original = o;
                Normal = normal;
                NoQuote = noQuote;
            }

            public T Original;
            public string Normal;
            public string NoQuote;
        }
        internal class TestSetV: TestSetBase<OneValue>
        {
            public TestSetV(OneValue o, string normal, string noQuote = null) : base(o, normal, noQuote) { }
        }
        internal class TestSetS: TestSetBase<string>
        {
            public TestSetS(string o, string normal, string noQuote = null) : base(o, normal, noQuote) { }
        }

        internal static List<TestSetV> SimpleValues = new List<TestSetV>
        {
            //new TestSet("('Val'!'...')", new OneValue("...") ),
            new TestSetV(new OneValue("simple"), "('Val'!'simple')", "(Val!simple)"),
            new TestSetV(new OneValue(8), "('Val'!8)", "(Val!8)"),
            new TestSetV(new OneValue("single quote '"), "('Val'!'single_quote_\\'')"),
            new TestSetV(new OneValue("multi 'single quote'"), "('Val'!'multi_\\'single_quote\\'')"),
            new TestSetV(new OneValue("multi single ''") , "('Val'!'multi_single_\\'\\'')"),
            new TestSetV(new OneValue("double \"") , "('Val'!'double_\"')"),
            new TestSetV(new OneValue("slash-quote\\\"") , "('Val'!'slash-quote\\\\\"')"),
            //new TestSet("('Val'!'slash-space\\_')", new OneValue("slash-space\\ ") ), // <-- todo: problem! looks like a \_ when encoded
            //new TestSet("('Val'!'slash-underline\\_')", new OneValue("slash-underline_") ),
            //new TestSet("('Val'!'slash-underline\\_')", new OneValue("slash-underline\\_") ),
            new TestSetV(new OneValue("text with {curly} and (normal) brackets") , "('Val'!'text_with_{curly}_and_(normal)_brackets')"),
            new TestSetV(new OneValue("mixed \" ' quotes \" '") , "('Val'!'mixed_\"_\\'_quotes_\"_\\'')"),
            new TestSetV(new OneValue("containing square [ brackets ]") , "('Val'!'containing_square_[_brackets_]')"),
            new TestSetV(new OneValue(new [] {1,2,3,-4,-22}) , "('Val'!L1*2*3*-4*-22J)"),
            new TestSetV(new OneValue(null) , "('Val'!n)"),
            new TestSetV(new OneValue(true) , "('Val'!t)"),
            new TestSetV(new OneValue(false) , "('Val'!f)"),
        };

        internal static List<TestSetS> Compactify = new List<TestSetS>
        {
            new TestSetS("{\"Val\" : \"simple\"}", "('Val'!'simple')"),
            new TestSetS("{\"Val\" : \"simple-containing-quote\\\"\"}", "('Val'!'simple-containing-quote\"')"),
            new TestSetS("   {\"Val\" : \"simple2\"}", "('Val'!'simple2')"),
            new TestSetS("   {  \"Val\" : \"simple3\"}", "('Val'!'simple3')"),
            new TestSetS("   {  \"Val\" \n\n: \"simple4\"}", "('Val'!'simple4')"),
            //new TestSet("('Val'!)", "{\"Val\"}"),
        };

        [TestMethod]
        public void SingleValuesEncode()
        {
            foreach (var test in SimpleValues)
            {
                var json = JsonConvert.SerializeObject(test.Original);
                var asGet = Json4Get.Encode(json);
                Assert.AreEqual(test.Normal, asGet, $"trouble with:{test.Original.Val}");
            }
        }

        [TestMethod]
        public void SingleValuesEncodeLighter()
        {
            foreach (var test in SimpleValues.Where(s => s.NoQuote != null))
            {
                var json = JsonConvert.SerializeObject(test.Original);
                var asGet = Json4Get.Encode(json, true);
                Assert.AreEqual(test.NoQuote, asGet, $"trouble with:{test.Original.Val}");
            }
        }
      [TestMethod]
        public void SingleValuesRecode()
        {
            foreach (var test in SimpleValues)
            {
                var json = JsonConvert.SerializeObject(test.Original);
                var asGet = Json4Get.Encode(json);
                var decoded = Json4Get.Decode(asGet);
                Assert.AreEqual(json, decoded, $"should be like the original `{test.Original.Val}`");
            }
        }

        [TestMethod]
        public void CompactifyEncode()
        {
            foreach (var test in Compactify)
            {
                var asGet = Json4Get.Encode(test.Original);
                Assert.AreEqual(test.Normal, asGet, $"trouble with:{test.Original}");
            }
        }


        [TestMethod]
        public void EncodeRecodeObject()
        {
            var test = new
            {
                Key = "something",
                Value = "something else",
                SomeArray = new[] { "a string", "another string" },
                IntArr = new[] { 7, 8, 8 },
                Is = true,
                Isnt = false,
                Null = null as string
            };
            var expectedJson4Get = "('Key'!'something'*'Value'!'something_else'*"
                + "'SomeArray'!L'a_string'*'another_string'J*"
                + "'IntArr'!L7*8*8J*"
                + "'Is'!t*'Isnt'!f*'Null'!n)";
            var json = JsonConvert.SerializeObject(test);
            var result = Json4Get.Encode(json);
            Assert.AreEqual(expectedJson4Get, result);
            var back = Json4Get.Decode(result);
            Assert.AreEqual(json, back, "convert back should work");
        }

        public class OneValue
        {
            public object Val;
            public OneValue(object value) => Val = value;
        }

    }
}
