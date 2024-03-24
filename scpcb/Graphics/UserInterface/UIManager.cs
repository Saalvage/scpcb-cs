using System.Numerics;
using scpcb.Entities;
using scpcb.Graphics.Textures;

namespace scpcb.Graphics.UserInterface;

public class UIManager : IRenderable {
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
}
