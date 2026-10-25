using NUnit.Framework;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class PathGuardTests
    {
        [TestCase(@"C:\Staging\123",            @"C:\Staging",   true)]
        [TestCase(@"C:\Staging\123\",           @"C:\Staging\",  true)]
        [TestCase(@"c:\staging\123",            @"C:\Staging",   true)]
        [TestCase(@"C:\Staging",                @"C:\Staging",   false)]
        [TestCase(@"C:\Staging\123\sub",        @"C:\Staging",   false)]
        [TestCase(@"C:\Other\123",              @"C:\Staging",   false)]
        [TestCase(@"C:\StagingEvil\123",        @"C:\Staging",   false)]
        [TestCase(@"C:\Staging\..\Other\123",   @"C:\Staging",   false)]
        [TestCase(@"C:\",                       @"C:\Staging",   false)]
        [TestCase("",                           @"C:\Staging",   false)]
        [TestCase(@"C:\Staging\123",            "",              false)]
        [TestCase(null,                         null,            false)]
        public void OnlyDirectChildrenCount(string folder, string parent, bool expected)
        {
            Assert.That(ModUtilities.IsDirectlyInside(folder, parent), Is.EqualTo(expected));
        }
    }
}