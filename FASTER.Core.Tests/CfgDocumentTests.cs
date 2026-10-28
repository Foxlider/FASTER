using FASTER.Services;
using NUnit.Framework;

namespace FASTER.Core.Tests;

[TestFixture]
public class CfgDocumentTests
{
    [Test]
    public void CommentsInsideStringsAndEscapedQuotesRemainLiteral()
    {
        const string text = """
            // hostname="Ignored";
            hostname="A // /* ""quoted"" server"; /* comment */
            motd[]={"First", /* second item */ "https://example.test/path"};
            """;
        var document = new CfgDocument(text);
        Assert.That(document.Entries, Has.Count.EqualTo(2));
        Assert.That(CfgDocument.StringValue(document.Entries[0].Values.Single()), Is.EqualTo("A // /* \"quoted\" server"));
        Assert.That(document.Raw(document.Entries[1]), Does.Contain("/* second item */"));
    }

    [Test]
    public void ForwardDeclarationsAndInheritedClassesKeepTheCorrectBoundaries()
    {
        const string text = "; class Base; class Derived:Base { value=42; class Child { text=\"x\"; }; }; tail=1;";
        var document = new CfgDocument(text);
        Assert.That(document.Entries.Select(e => e.Name), Is.EqualTo(new[] { "Derived", "tail" }));
        var derived = document.Entries[0];
        Assert.That(derived.Children!.Select(e => e.Name), Is.EqualTo(new[] { "value", "Child" }));
        Assert.That(document.Raw(derived), Is.EqualTo("class Derived:Base { value=42; class Child { text=\"x\"; }; };"));
    }

    [TestCase("value=1;\n/* unfinished", "Unclosed comment (line 2)")]
    [TestCase("value=1;\ntext=\"unfinished", "Unclosed string (line 2)")]
    [TestCase("value=1;\nclass Bad=value;", "Expected class body (line 2)")]
    [TestCase("value=1;\n}", "Unexpected closing brace (line 2)")]
    public void MalformedInputReportsTheOriginalLine(string text, string error)
    {
        Assert.That(() => new CfgDocument(text), Throws.TypeOf<FormatException>().With.Message.Contains(error));
    }

    [Test]
    public void MergeAppliesCollectedEditsInSourceOrderAndPreservesCustomText()
    {
        const string source = "// custom header\nmaxPlayers=32; MAXPLAYERS=32; class Options { retained=9; enabled=0; }; custom[]={1,2};";
        const string before = "maxPlayers=32; class Options { enabled=0; };";
        const string after = "maxPlayers=225; class Options { enabled=1; added=2; }; newSetting=3;";
        string merged = CfgDocument.Merge(source, before, after);
        Assert.That(merged, Does.StartWith("// custom header\nmaxPlayers=225; MAXPLAYERS=225;")
            .And.Contain("retained=9;").And.Contain("enabled=1;").And.Contain("added=2;")
            .And.Contain("custom[]={1,2};").And.Contain("newSetting=3;"));
        Assert.DoesNotThrow(() => new CfgDocument(merged));
    }

    [Test]
    public void MergeRemovesOptionalBlocksAndReplacesMissionOverridesAsAWhole()
    {
        const string source = "class AntiFlood { extra=5; cycleTime=1; }; class Missions { class Old { template=\"old\"; }; }; custom=42;";
        const string before = "class AntiFlood { cycleTime=1; }; class Missions {};";
        const string after = "class Missions { class New { template=\"new\"; }; };";
        string merged = CfgDocument.Merge(source, before, after);
        Assert.That(merged, Does.Not.Contain("AntiFlood").And.Not.Contain("class Old")
            .And.Contain("class New").And.Contain("custom=42;"));
    }

    [Test]
    public void UnchangedGeneratedSettingsLeaveThePastedTextUntouched()
    {
        const string source = "// my formatting\nvalue = 42; custom=7;";
        Assert.That(CfgDocument.Merge(source, "value=42;", "value=42;"), Is.EqualTo(source));
    }
}
