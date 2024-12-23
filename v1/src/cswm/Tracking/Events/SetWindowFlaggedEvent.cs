using cswm.Events;
using cswm.WinApi;

namespace cswm.Tracking.Events;

public class SetWindowFlaggedEvent : Event
{
    private readonly Window _window;
    private readonly bool _flagged;

    public SetWindowFlaggedEvent(Window window, bool ignore = true)
    {
        _window = window;
        _flagged = ignore;
    }

    public Window Window => _window;
    public bool Flagged => _flagged;
}
