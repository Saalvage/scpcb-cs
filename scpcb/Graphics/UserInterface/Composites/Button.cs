﻿using System.Drawing;
using System.Numerics;
using scpcb.Graphics.UserInterface.Primitives;
using scpcb.Graphics.UserInterface.Utility;
using Veldrid;

namespace scpcb.Graphics.UserInterface.Composites;

public class Button : InteractableUIElement<MenuFrame> {
    private readonly IUIElement _hover;

    public event Action OnClicked;

    public Button(GraphicsResources gfxRes, UIManager ui, string text, float outerXOff, float innerXOff, float yOff)
            : base(new(gfxRes, ui, outerXOff, innerXOff, yOff)) {
        PixelSize = new(512, 64);
        _internalChildren.Add(_hover = new TextureElement(ui.GraphicsResources,
            ui.GraphicsResources.TextureCache.GetTexture(Color.FromArgb(30, 30, 30))) {
            Alignment = Alignment.Center,
            PixelSize = PixelSize - new Vector2(8),
            IsVisible = false,
        });
        _internalChildren.Add(new TextElement(gfxRes, gfxRes.FontCache.GetFont("Assets/Fonts/Courier New.ttf", 64)) {
            Text = text,
            Alignment = Alignment.Center,
        });
    }
    
    protected override void OnBeginHover() {
        _hover.IsVisible = true;
    }

    protected override void OnEndHover() {
        _hover.IsVisible = false;
    }

    protected override void OnMouseDown(MouseButton button, Vector2 pos) {
        OnClicked?.Invoke();
    }
}