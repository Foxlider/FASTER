using System.Threading;
using System.Threading.Tasks;
using FASTER.Services;
using Velopack;
using Velopack.Sources;

namespace FASTER.Avalonia.Services;

internal sealed class VelopackUpdates : IApplicationUpdateBackend
{
    private const string ReleaseRepository = "https://github.com/milutinke/FASTER";
    public const string ReleasePage = ReleaseRepository + "/releases";
    private UpdateManager? _manager;
    private UpdateInfo? _update;
    private UpdateManager Manager => _manager ??= new UpdateManager(new GithubSource(ReleaseRepository, null, false));
    public bool IsInstalled => Manager.IsInstalled;
    public async Task<bool> CheckAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _update = await Manager.CheckForUpdatesAsync();
        cancellationToken.ThrowIfCancellationRequested();
        return _update != null;
    }
    public Task DownloadAsync(CancellationToken cancellationToken) => Manager.DownloadUpdatesAsync(_update!, cancelToken: cancellationToken);
    public void ApplyAndRestart() => Manager.ApplyUpdatesAndRestart(_update!);
}
