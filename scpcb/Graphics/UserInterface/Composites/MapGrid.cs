﻿using System.Drawing;
using System.Numerics;
using scpcb.Graphics.UserInterface.Primitives;
using Veldrid;

namespace scpcb.Graphics.UserInterface.Composites;

public class MapGrid : InteractableUIElement<UIElement> {
    private const int TILE_SIZE = 32;
    private const int OFFSET = 1;

    private readonly int _size;
    private readonly int _internalStart;

    private readonly GraphicsResources _gfx;

    private readonly TextureElement _activeMarker;

    public MapGrid(GraphicsResources gfx, int size) : base(new() { PixelSize = new(TILE_SIZE + (size - 1) * (TILE_SIZE + OFFSET)), }) {
        _gfx = gfx;
        _size = size;

        _internalStart = _internalChildren.Count;
        foreach (var x in Enumerable.Range(0, size)) {
            foreach (var y in Enumerable.Range(0, size)) {
                var tile = new MapTile(gfx, TILE_SIZE) {
                    Position = new((TILE_SIZE + OFFSET) * x, (TILE_SIZE + OFFSET) * y),
                };
                _internalChildren.Add(tile);
            }
        }

        _internalChildren.Add(_activeMarker = new(gfx, gfx.TextureCache.GetTexture(Color.White)) {
            PixelSize = new(TILE_SIZE),
            Z = -1,
            Color = Color.FromArgb(255, 0xC8, 0xC8, 0xC8),
        });
    }

    public override void OnUpdate(Vector2 pos, InputSnapshot snapshot) {
        var (x, y) = GetIndices(snapshot.MousePosition);

        if (x >= 0 && x < _size && y >= 0 && y < _size) {
            _activeMarker.Position = new Vector2(x, y) * (TILE_SIZE + OFFSET);
        }
    }

    protected override void OnMouseDown(MouseButton button, Vector2 pos) {
        if (button is not (MouseButton.Left or MouseButton.Right)) {
            return;
        }

        var (x, y) = GetIndices(pos);

        ((MapTile)_internalChildren[_internalStart + x * _size + y]).Tile = button == MouseButton.Left
            ? new TextureElement(_gfx, _gfx.TextureCache.GetTexture("Assets/MapCreator/room2.png")) {
                PixelSize = new(TILE_SIZE),
            } : null;
    }

    private (int, int) GetIndices(Vector2 pos) {
        return ((int)(pos.X / (TILE_SIZE + OFFSET)), (int)(pos.Y / (TILE_SIZE + OFFSET)));
    }
}
