using System.Numerics;
using SCPCB.Entities;
using SCPCB.Graphics.Textures;
using SCPCB.Graphics.UserInterface.Primitives;
using Veldrid.Sdl2;

namespace SCPCB.Graphics.UserInterface;

// TODO: Some thoughts on the UI in general:
// - Composite elements might not always support all inherited properties that they expose (e.g. inability to rescale via setting PixelSize).
// - The goal should be to optimize the entire system by batching sprites together.
public class UIManager : IRenderable, IUpdatable {
    public class RootElement : UIElement {
        public void Visit(Func<IUIElement, Vector2, bool> visitor, bool visitInvisible = false)
            => Visit(this, PixelSize / 2, visitor, visitInvisible);
    }

    public RootElement Root { get; }

    public float MenuScale { get; }

    public int Priority => 200000;

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
        Root.Draw(target, Root, new Vector2(GraphicsResources.Window.Width, GraphicsResources.Window.Height) / 2, 0);
    }

    public void Update(float delta) {
        Root.Visit((elem, pos) => {
            if (elem is IInteractableUIElement interactive) {
                interactive.Update(pos - elem.PixelSize / 2, InputManager.Snapshot);
            }

            return true;
        });
    }

    public enum CursorStyle {
        Default,
        Text,
        Click,
    }

    private static readonly SDL_Cursor CURSOR_ARROW = Sdl2Native.SDL_CreateSystemCursor(SDL_SystemCursor.Arrow);
    private static readonly SDL_Cursor CURSOR_IBEAM = Sdl2Native.SDL_CreateSystemCursor(SDL_SystemCursor.IBeam);
    private static readonly SDL_Cursor CURSOR_HAND = Sdl2Native.SDL_CreateSystemCursor(SDL_SystemCursor.Hand);

    // TODO: Implement this stack-based to prevent overrides.
    public void SetCursorStyle(CursorStyle style) {
        Sdl2Native.SDL_SetCursor(style switch {
            CursorStyle.Default => CURSOR_ARROW,
            CursorStyle.Text => CURSOR_IBEAM,
            CursorStyle.Click => CURSOR_HAND,
        });
    }
}
