using System;
using System.Threading;

internal static class UiDispatcher
{
    private static SynchronizationContext _context;

    public static void Init()
    {
        _context = SynchronizationContext.Current
                   ?? throw new InvalidOperationException(
                       "UiDispatcher.Init() muss im UI-Thread aufgerufen werden.");
    }

    public static void Post(Action action)
    {
        if (action == null) return;

        var ctx = _context;
        if (ctx == null)
        {
            // Fallback: Wenn Init vergessen wurde, einfach direkt ausführen
            action();
            return;
        }

        ctx.Post(_ => action(), null);
    }

    public static void Send(Action action)
    {
        if (action == null) return;

        var ctx = _context;
        if (ctx == null)
        {
            action();
            return;
        }

        ctx.Send(_ => action(), null);
    }
}
