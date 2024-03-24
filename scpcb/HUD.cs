using System.Drawing;
using scpcb.Entities;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.UserInterface;

namespace scpcb;

public class HUD : IEntity {
    private readonly UIManager _ui;

    private TextureElement? _singleHeadsUpItem;

    public HUD(UIManager ui) {
        _ui = ui;
        var gfxRes = _ui.GraphicsResources;

        const int X = 30; const int Y = -65;

        var blinkBar = new BorderedImage(gfxRes, new(32), 1, Color.White,
                gfxRes.TextureCache.GetTexture("Assets/Textures/HUD/BlinkIcon.png")) {
            Alignment = Alignment.BottomLeft,
            Position = new(X - 1, Y + 1),
        };
        blinkBar.Children.Add(new LoadingBar(gfxRes, 20, gfxRes.TextureCache.GetTexture("Assets/Textures/HUD/BlinkMeter.jpg")) {
            Position = new(1 + 50, 1),
        });
        _ui.Root.Children.Add(blinkBar);

        var staminaBar = new BorderedImage(gfxRes, new(32), 1, Color.White,
                gfxRes.TextureCache.GetTexture("Assets/Textures/HUD/sprinticon.png")) {
            Alignment = Alignment.BottomLeft,
            Position = new(X - 1, Y + 1 + 40),
        };
        staminaBar.Children.Add(new LoadingBar(gfxRes, 20, gfxRes.TextureCache.GetTexture("Assets/Textures/HUD/StaminaMeter.jpg")) {
            Position = new(1 + 50, 1),
        });
        _ui.Root.Children.Add(staminaBar);
    }

    public void SetItem(ICBTexture texture) {
        ClearItem();
        _singleHeadsUpItem = new(_ui.GraphicsResources, texture) { Alignment = Alignment.Center, Z = 1 };
        _singleHeadsUpItem.PixelSize *= _ui.MenuScale;
        _ui.Root.Children.Add(_singleHeadsUpItem);
    }

    public void ClearItem() {
        if (_singleHeadsUpItem != null) {
            _ui.Root.Children.Remove(_singleHeadsUpItem);
            _singleHeadsUpItem = null;
        }
    }
}
