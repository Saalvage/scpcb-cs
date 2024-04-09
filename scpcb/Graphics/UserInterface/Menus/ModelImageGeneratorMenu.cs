using scpcb.Graphics.UserInterface.Primitives;

namespace scpcb.Graphics.UserInterface.Menus;

public class ModelImageGeneratorMenu : UIElement {
    public ModelImageGeneratorMenu(GraphicsResources gfxRes) {
        PixelSize = new(gfxRes.Window.Width, gfxRes.Window.Height);
    }
}
