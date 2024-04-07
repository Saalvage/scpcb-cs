using System.Numerics;
using scpcb.Entities;
using scpcb.Graphics.Textures;
using scpcb.Scenes;

namespace scpcb.Graphics.UserInterface;

// TODO: Some thoughts on the UI in general:
// - Composite elements might not always support all inherited properties that they expose (e.g. inability to rescale via setting PixelSize).
// - Z should likely accumulate while descending the hierarchy.
// - The goal should be to optimize the entire system by batching sprites together.
public class UIManager : IRenderable, IUpdatable {
    public IUIElement Root { get; }

    public float MenuScale { get; }

    public int Priority => 200000;

    // TODO: Do we need to expose this? Moving it to IScene seems like a possibility.
    public GraphicsResources GraphicsResources { get; }

    public UIManager(GraphicsResources gfxRes) {
        GraphicsResources = gfxRes;
        Root = new UIElement { PixelSize = new(gfxRes.Window.Width, gfxRes.Window.Height) };
        MenuScale = Math.Min(gfxRes.Window.Height, gfxRes.Window.Width) / 1024f;
    }

    public void Render(IRenderTarget target, float interp) {
        target.ClearDepthStencil();
        Root.Draw(target, Root, Vector2.Zero);
    }

    public void Update(float delta) {
        var mousePos = MainScene.MousePos;

        Root.Visit(Root, Vector2.Zero, (elem, pos) => {
            if (!elem.IsVisible) {
                return false;
            }

            pos += 0.5f * new Vector2(GraphicsResources.Window.Width, -GraphicsResources.Window.Height);
            pos -= elem.PixelSize / 2 * new Vector2(1, -1);
            pos.Y = -pos.Y;

            if (elem is IInteractableUIElement interactive) {
                var hovering = pos.X <= mousePos.X && pos.X + elem.PixelSize.X >= mousePos.X
                                                   && pos.Y <= mousePos.Y && pos.Y + elem.PixelSize.Y >= mousePos.Y;
                if (interactive.Hovering && !hovering) {
                    interactive.OnEndHover();
                } else if (!interactive.Hovering && hovering) {
                    interactive.OnBeginHover();
                }

                interactive.Hovering = hovering;
            }

            return true;
        });
    }
}
