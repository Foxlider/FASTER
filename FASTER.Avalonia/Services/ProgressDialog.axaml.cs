using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Threading;

using FASTER.Services;

namespace FASTER.Avalonia.Services;

public partial class ProgressDialog : Window, IProgressDialog
{
    private readonly TaskCompletionSource _closed = new();

    public ProgressDialog() => InitializeComponent();

    public new double Maximum
    {
        get => Bar.Maximum;
        set => Dispatcher.UIThread.Post(() => Bar.Maximum = value);
    }

    public void SetMessage(string message) => Dispatcher.UIThread.Post(() => MessageText.Text = message);
    public void SetProgress(double value) => Dispatcher.UIThread.Post(() => Bar.Value = value);

    public Task CloseAsync()
    {
        Dispatcher.UIThread.Post(Close);
        return _closed.Task;
    }

    protected override void OnClosed(System.EventArgs e)
    {
        base.OnClosed(e);
        _closed.TrySetResult();
    }
}
