using System.Drawing;
using System.Numerics;

namespace scpcb.Graphics.UserInterface;

public class Button : MenuFrame, IInteractableUIElement {
    private readonly IUIElement _hover;

    public Button(GraphicsResources gfxRes, UIManager ui, string text, float outerXOff, float innerXOff, float yOff)
            : base(gfxRes, ui, outerXOff, innerXOff, yOff) {
        PixelSize = new(512, 64);
        _internalChildren.Add(_hover = new TextureElement(_ui.GraphicsResources,
            _ui.GraphicsResources.TextureCache.GetTexture(Color.FromArgb(30, 30, 30))) {
            Alignment = Alignment.Center,
            PixelSize = PixelSize - new Vector2(8),
            IsVisible = false,
        });
        _internalChildren.Add(new TextElement(gfxRes, gfxRes.FontCache.GetFont("Assets/Fonts/Courier New.ttf", 64)) {
            Text = text,
            Alignment = Alignment.Center,
        });
    }
    
    public void OnBeginHover() {
        _hover.IsVisible = true;
    }

    public void OnEndHover() {
        _hover.IsVisible = false;
    }

    public bool Hovering { get; set; }
}
