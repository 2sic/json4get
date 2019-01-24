using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Json4Get;

namespace Json4Get_Tests
{
    [TestClass]
    public class Tests_ConfigurationData
    {
        [TestMethod]
        public void VerifyArrayLengths()
        {
            // all these arrays MUST have the same length, or we made a mistake
            Assert.AreEqual(Characters.Specials.Length, Characters.Replacements.Length, "replacements");
            Assert.AreEqual(Characters.Specials.Length, Characters.StartsValue.Length, "startsWith");
            Assert.AreEqual(Characters.Specials.Length, Characters.OpenCounters.Length, "openCounters");

            Assert.AreEqual(Characters.JsonStartMarkers.Length, Characters.Json4GetStartMarkers.Length, "json start markers");
        }
    }
}
