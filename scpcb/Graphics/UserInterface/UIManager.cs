using System.Numerics;
using scpcb.Entities;
using scpcb.Graphics.Textures;
using scpcb.Graphics.UserInterface.Primitives;
using Veldrid.Sdl2;

namespace scpcb.Graphics.UserInterface;

// TODO: Some thoughts on the UI in general:
// - Composite elements might not always support all inherited properties that they expose (e.g. inability to rescale via setting PixelSize).
// - Z should likely accumulate while descending the hierarchy.
// - The goal should be to optimize the entire system by batching sprites together.
public class UIManager : IRenderable, IUpdatable {
    public class RootElement : UIElement {
        public void Visit(Func<IUIElement, Vector2, bool> visitor, bool visitInvisible = false)
            => Visit(this, Vector2.Zero, visitor, visitInvisible);
    }

    public RootElement Root { get; }

    public float MenuScale { get; }

    public int Priority => 200000;

    // TODO: Do we need to expose this? Moving it to IScene seems like a possibility.
    public GraphicsResources GraphicsResources { get; }

    public InputManager InputManager { get; }

    public UIManager(GraphicsResources gfxRes, InputManager input) {
        GraphicsResources = gfxRes;
        InputManager = input;
        Root = new() { PixelSize = new(gfxRes.Window.Width, gfxRes.Window.Height) };
        MenuScale = Math.Min(gfxRes.Window.Height, gfxRes.Window.Width) / 1024f;
    }

    public void Render(IRenderTarget target, float interp) {
        target.ClearDepthStencil();
        Root.Draw(target, Root, Vector2.Zero);
    }

    public void Update(float delta) {
        Root.Visit((elem, pos) => {
            if (elem is IInteractableUIElement interactive) {
                interactive.Update(UnfuckCoordinates(elem, pos), InputManager.Snapshot);
            }

            return true;
        });
    }

    private Vector2 UnfuckCoordinates(IUIElement elem, Vector2 pos) {
        pos += 0.5f * new Vector2(GraphicsResources.Window.Width, -GraphicsResources.Window.Height);
        pos.Y = -pos.Y;
        pos -= elem.PixelSize / 2;
        return pos;
    }

    public enum CursorStyle {
        Default,
        Text,
        Click,
    }

    private static readonly SDL_Cursor CURSOR_ARROW = Sdl2Native.SDL_CreateSystemCursor(SDL_SystemCursor.Arrow);
    private static readonly SDL_Cursor CURSOR_IBEAM = Sdl2Native.SDL_CreateSystemCursor(SDL_SystemCursor.IBeam);
    private static readonly SDL_Cursor CURSOR_HAND = Sdl2Native.SDL_CreateSystemCursor(SDL_SystemCursor.Hand);

    public void SetCursorStyle(CursorStyle style) {
        Sdl2Native.SDL_SetCursor(style switch {
            CursorStyle.Default => CURSOR_ARROW,
            CursorStyle.Text => CURSOR_IBEAM,
            CursorStyle.Click => CURSOR_HAND,
        });
    }
}
