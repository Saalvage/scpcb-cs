using Veldrid;
using Veldrid.Sdl2;

namespace SCPCB;

public class InputManager {
    private readonly Sdl2Window _window;

    public InputSnapshot Snapshot { get; private set; }

    private readonly Dictionary<Key, bool> _keysDown = [];
    public bool IsKeyDown(Key x) => _keysDown.TryGetValue(x, out var y) && y;

    private readonly Dictionary<MouseButton, bool> _mouseButtonsDown = [];
    public bool IsMouseButtonDown(MouseButton x) => _mouseButtonsDown.TryGetValue(x, out var y) && y;

    public InputManager(Sdl2Window window) {
        _window = window;
    }

    public void PumpEvents() {
        Snapshot = _window.PumpEvents();
        foreach (var ev in Snapshot.KeyEvents) {
            _keysDown[ev.Key] = ev.Down;
        }

        foreach (var ev in Snapshot.MouseEvents) {
            _mouseButtonsDown[ev.MouseButton] = ev.Down;
        }
    }

    public void SetMouseCaptured(bool isCaptured) {
        Sdl2Native.SDL_SetRelativeMouseMode(isCaptured);
        if (!isCaptured) {
            Sdl2Native.SDL_WarpMouseInWindow(_window.SdlWindowHandle, _window.Width / 2, _window.Height / 2);
        }
        Sdl2Native.SDL_ShowCursor(isCaptured ? 0 : 1);
    }
}
