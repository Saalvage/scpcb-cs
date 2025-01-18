using System.Drawing;
using System.Numerics;
using SCPCB.Graphics.UserInterface.Primitives;
using SCPCB.Graphics.UserInterface.Utility;
using Veldrid;

namespace SCPCB.Graphics.UserInterface.Composites;

public class Button : InteractableUIElement<Label> {
    private readonly UIManager _ui;

    private readonly TextureElement _hover;

    public event Action? OnClicked;

    public Button(GraphicsResources gfxRes, UIManager ui, string text, float outerXOff, float innerXOff, float yOff, int textSize = 64)
            : base(new(gfxRes, ui, outerXOff, innerXOff, yOff, textSize)) {
        _ui = ui;
        _internalChildren.Add(_hover = new(ui.GraphicsResources,
            ui.GraphicsResources.TextureCache.GetTexture(Color.FromArgb(30, 30, 30))) {
            Alignment = Alignment.Center,
            IsVisible = false,
        });
        PixelSize = new(512, 64);
        Inner.Text.Z = 1;
        Inner.Text.Text = text;
    }

    public override Vector2 PixelSize {
        get => base.PixelSize;
        set {
            _hover.PixelSize = value - new Vector2(8);
            base.PixelSize = value;
        }
    }

    protected override void OnBeginHover(InputSnapshot snapshot) {
        _hover.IsVisible = true;
        _ui.SetCursorStyle(UIManager.CursorStyle.Click);
    }

    protected override void OnEndHover(InputSnapshot snapshot) {
        _hover.IsVisible = false;
        _ui.SetCursorStyle(UIManager.CursorStyle.Default);
    }

    protected override void OnMouseDown(MouseButton button, Vector2 pos) {
        OnClicked?.Invoke();
    }
}
