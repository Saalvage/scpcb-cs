﻿using System.Drawing;
using scpcb.Entities;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.UserInterface;
using scpcb.Graphics.UserInterface.Composites;
using scpcb.Graphics.UserInterface.Menus;
using scpcb.Graphics.UserInterface.Primitives;
using scpcb.Graphics.UserInterface.Utility;
using scpcb.PlayerController;

namespace scpcb;

public class HUD : IUpdatable {
    private readonly Player _player;
    private readonly UIManager _ui;

    // Exists to prevent issues of modifying children while enumerating.
    private readonly UIElement _headsUpTextureDummy;

    private TextureElement? _singleHeadsUpItem;

    private readonly LoadingBar _blinkBar;
    private readonly LoadingBar _staminaBar;

    public InventoryMenu Inventory { get; }

    public HUD(Player player, UIManager ui) {
        _player = player;
        _ui = ui;
        var gfxRes = _ui.GraphicsResources;

        _headsUpTextureDummy = new() { Alignment = Alignment.Center };
        _ui.Root.AddChild(_headsUpTextureDummy);

        const int X = 30; const int Y = -65;

        var blinkIcon = new BorderedImage(gfxRes, new(32), 1, Color.White,
                gfxRes.TextureCache.GetTexture("Assets/Textures/HUD/BlinkIcon.png")) {
            Alignment = Alignment.BottomLeft,
            Position = new(X - 1, Y + 1),
        };
        _blinkBar = new(gfxRes, 20, gfxRes.TextureCache.GetTexture("Assets/Textures/HUD/BlinkMeter.jpg")) {
            Position = new(1 + 50, 1),
        };
        blinkIcon.AddChild(_blinkBar);
        _ui.Root.AddChild(blinkIcon);

        var staminaIcon = new BorderedImage(gfxRes, new(32), 1, Color.White,
                gfxRes.TextureCache.GetTexture("Assets/Textures/HUD/sprinticon.png")) {
            Alignment = Alignment.BottomLeft,
            Position = new(X - 1, Y + 1 + 40),
        };
        _staminaBar = new(gfxRes, 20, gfxRes.TextureCache.GetTexture("Assets/Textures/HUD/StaminaMeter.jpg")) {
            Position = new(1 + 50, 1),
        };
        staminaIcon.AddChild(_staminaBar);
        _ui.Root.AddChild(staminaIcon);

        _ui.Root.AddChild(Inventory = new(gfxRes, ui, player.Items) {
            Alignment = Alignment.Center,
            IsVisible = false,
        });
    }

    public void SetItem(ICBTexture texture) {
        ClearItem();
        _singleHeadsUpItem = new(_ui.GraphicsResources, texture) { Alignment = Alignment.Center, Z = 1 };
        _singleHeadsUpItem.PixelSize *= _ui.MenuScale;
        _headsUpTextureDummy.AddChild(_singleHeadsUpItem);
    }

    public void ClearItem() {
        if (_singleHeadsUpItem != null) {
            _headsUpTextureDummy.RemoveChild(_singleHeadsUpItem);
            _singleHeadsUpItem = null;
        }
    }

    public void Update(float delta) {
        _blinkBar.BarCount = 10;
        _staminaBar.SetProgress(_player.Stamina / _player.MaxStamina, ProgressHandling.Ceiling);
        if (Inventory.IsVisible) {
            Inventory.Update(_player.Items);
        }
    }
}
