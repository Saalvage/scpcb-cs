using System.Numerics;
using scpcb.Utility;
using Veldrid.Sdl2;

namespace scpcb;

public class InputManager {
    private readonly Sdl2Window _window;

    public Vector2 MousePosition { get; private set; }

    public InputManager(Sdl2Window window) {
        _window = window;
    }

    private void MouseMove(MouseMoveEventArgs args) {
        MousePosition = args.MousePosition;
    }

    public void PumpEvents() {
        var snapshot = _window.PumpEvents();
        MousePosition = snapshot.MousePosition;
    }

    public void SetMouseCaptured(bool isCaptured) {
        Sdl2Native.SDL_SetRelativeMouseMode(isCaptured);
        if (!isCaptured) {
            Sdl2Native.SDL_WarpMouseInWindow(_window.SdlWindowHandle, _window.Width / 2, _window.Height / 2);
        }
        Sdl2Native.SDL_ShowCursor(isCaptured ? 0 : 1);
    }
}
