using NUnit.Framework;

using System;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class VersionFormatTests
    {
        [TestCase("1.9.8.0",    "1.9h")]
        [TestCase("1.9.8.42",   "1.9h")]
        [TestCase("1.9.8.101",  "1.9h H1")]
        [TestCase("1.9.8.110",  "1.9h H10")]
        [TestCase("1.9.9.205",  "1.9i RC5")]
        [TestCase("1.10.1.503", "1.10a D3")]
        [TestCase("1.9.0.0",    "1.9ALPHA")]
        public void VersionsAreFormatted(string input, string expected)
        {
            Assert.That(Functions.FormatVersion(new Version(input)), Is.EqualTo(expected));
        }

        [Test()]
        public void DevSuffixGoesLast()
        {
            Assert.That(Functions.FormatVersion(new Version("1.9.8.0"),   isDev: true), Is.EqualTo("1.9h-DEV"));
            Assert.That(Functions.FormatVersion(new Version("1.9.8.101"), isDev: true), Is.EqualTo("1.9h H1-DEV"));
        }

        [Test()]
        public void MissingVersionIsUnknown()
        {
            Assert.That(Functions.FormatVersion(null), Is.EqualTo("UNKNOWN"));
        }
    }
}