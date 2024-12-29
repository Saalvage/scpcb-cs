using SCPCB.Graphics.UserInterface.Primitives;
using SCPCB.Graphics.UserInterface.Utility;

namespace SCPCB.Graphics.UserInterface.Composites;

public class Label : MenuFrame {
    public TextElement Text { get; }

    public Label(GraphicsResources gfxRes, UIManager ui, float outerXOff, float innerXOff, float yOff, int textSize)
            : base(gfxRes, ui, outerXOff, innerXOff, yOff) {
        _internalChildren.Add(Text = new(gfxRes, gfxRes.FontCache.GetFont("Assets/Fonts/Courier New.ttf", textSize)) {
            Alignment = Alignment.Center,
        });
    }
}
