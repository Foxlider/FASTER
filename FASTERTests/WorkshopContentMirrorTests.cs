using FASTER.Services.SteamCmd;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FASTERTests;

[TestFixture]
public sealed class WorkshopContentMirrorTests
{
    private string testRoot = null!;

    [SetUp]
    public void SetUp()
    {
        testRoot = Path.Combine(Path.GetTempPath(), "FASTERTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(testRoot);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(testRoot))
        {
            Directory.Delete(testRoot, recursive: true);
        }
    }

    [Test]
    public async Task MirrorAsync_ReplacesExistingTargetOnlyAfterCopyingNewContent()
    {
        string source = CreateDirectory("steamcmd-source");
        string nestedSource = Directory.CreateDirectory(Path.Combine(source, "addons")).FullName;
        await File.WriteAllTextAsync(Path.Combine(nestedSource, "new.pbo"), "new content");

        string staging = CreateDirectory("staging");
        string oldTarget = Directory.CreateDirectory(Path.Combine(staging, "450814997")).FullName;
        await File.WriteAllTextAsync(Path.Combine(oldTarget, "obsolete.pbo"), "old content");

        WorkshopContentMirror mirror = new();
        string target = await mirror.MirrorAsync(source, staging, 450814997);

        Assert.Multiple(() =>
        {
            Assert.That(target, Is.EqualTo(Path.Combine(Path.GetFullPath(staging), "450814997")));
            Assert.That(File.ReadAllText(Path.Combine(target, "addons", "new.pbo")), Is.EqualTo("new content"));
            Assert.That(File.Exists(Path.Combine(target, "obsolete.pbo")), Is.False);
            Assert.That(Directory.EnumerateFileSystemEntries(staging, "*.incoming-*"), Is.Empty);
            Assert.That(Directory.EnumerateFileSystemEntries(staging, "*.backup-*"), Is.Empty);
        });
    }

    [Test]
    public void MirrorAsync_RejectsEmptySourceAndPreservesExistingTarget()
    {
        string source = CreateDirectory("empty-source");
        string staging = CreateDirectory("staging");
        string target = Directory.CreateDirectory(Path.Combine(staging, "1234")).FullName;
        File.WriteAllText(Path.Combine(target, "keep.txt"), "keep me");

        WorkshopContentMirror mirror = new();

        Assert.ThrowsAsync<InvalidDataException>(async () => await mirror.MirrorAsync(source, staging, 1234));
        Assert.That(File.ReadAllText(Path.Combine(target, "keep.txt")), Is.EqualTo("keep me"));
    }

    [Test]
    public void MirrorAsync_RejectsMissingSourceBeforeCreatingStaging()
    {
        string source = Path.Combine(testRoot, "missing-source");
        string staging = Path.Combine(testRoot, "staging");
        WorkshopContentMirror mirror = new();

        Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await mirror.MirrorAsync(source, staging, 1234));
        Assert.That(Directory.Exists(staging), Is.False);
    }

    [Test]
    public void MirrorAsync_HonorsCancellationBeforeChangingTheTarget()
    {
        string source = CreateDirectory("steamcmd-source");
        File.WriteAllText(Path.Combine(source, "new.pbo"), "new content");
        string staging = CreateDirectory("staging");
        string target = Directory.CreateDirectory(Path.Combine(staging, "9876")).FullName;
        File.WriteAllText(Path.Combine(target, "keep.pbo"), "old content");

        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        WorkshopContentMirror mirror = new();

        Assert.ThrowsAsync<OperationCanceledException>(
            async () => await mirror.MirrorAsync(source, staging, 9876, cancellation.Token));
        Assert.Multiple(() =>
        {
            Assert.That(File.ReadAllText(Path.Combine(target, "keep.pbo")), Is.EqualTo("old content"));
            Assert.That(Directory.GetDirectories(staging), Is.EquivalentTo(new[] { target }));
        });
    }

    [Test]
    public void MirrorAsync_RejectsStagingNestedInsideSource()
    {
        string source = CreateDirectory("source");
        File.WriteAllText(Path.Combine(source, "content.pbo"), "content");
        string staging = Path.Combine(source, "managed-mods");
        WorkshopContentMirror mirror = new();

        Assert.ThrowsAsync<InvalidOperationException>(async () => await mirror.MirrorAsync(source, staging, 42));
        Assert.That(Directory.Exists(staging), Is.False);
    }

    [Test]
    public void MirrorAsync_RejectsZeroWorkshopId()
    {
        string source = CreateDirectory("source");
        File.WriteAllText(Path.Combine(source, "content.pbo"), "content");
        string staging = CreateDirectory("staging");
        WorkshopContentMirror mirror = new();

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await mirror.MirrorAsync(source, staging, 0));
        Assert.That(Directory.EnumerateFileSystemEntries(staging), Is.Empty);
    }

    private string CreateDirectory(string name) => Directory.CreateDirectory(Path.Combine(testRoot, name)).FullName;
}
