using ErixOpti.Core.ViewModels;
using Microsoft.UI.Dispatching;
using Serilog.Core;
using Serilog.Events;

namespace ErixOpti.Services;

public sealed class UiLogSink : ILogEventSink
{
    private readonly LogViewModel _log;
    private DispatcherQueue? _queue;

    public UiLogSink(LogViewModel log)
    {
        _log = log;
    }

    public void Attach(DispatcherQueue queue) => _queue = queue;

    public void Emit(LogEvent logEvent)
    {
        var text = logEvent.RenderMessage();
        if (logEvent.Exception is not null)
        {
            text += Environment.NewLine + logEvent.Exception;
        }

        var q = _queue;
        if (q is not null)
        {
            q.TryEnqueue(() => _log.Append(text));
        }
    }
}
