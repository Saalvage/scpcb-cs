using System.Drawing;
using System.Numerics;
using SCPCB.Graphics.UserInterface.Primitives;
using SCPCB.Map;
using Veldrid;

namespace SCPCB.Graphics.UserInterface.Composites;

public class MapTile : InteractableUIElement<UIElement> {
    private readonly GraphicsResources _gfxRes;

    private readonly Border _border;

    private Vector2? _grabbedPos;

    private TextureElement? _tile;
    private TextureElement? Tile {
        set {
            if (_tile != null) {
                _internalChildren.Remove(_tile);
            }
            _tile = value;
            if (_tile != null) {
                _internalChildren.Add(_tile);
            }
            _border.IsVisible = _tile == null;
        }
    }

    private PlacedRoomInfo? _room;

    public PlacedRoomInfo? Room {
        get => _room;
        set {
            _room = value;
            Tile = value == null ? null : new(_gfxRes, _gfxRes.TextureCache.GetTexture($"Assets/MapCreator/{value.Room.Shape switch {
                Shape._1 => "room1",
                Shape._2 => "room2",
                Shape._2C => "room2C",
                Shape._3 => "room3",
                Shape._4 => "room4",
            }}.png")) {
                PixelSize = PixelSize,
            };
            if (value != null) {
                SetDirection(value.Direction);
            }
        }
    }

    public void GrabRotate(Vector2 pos) {
        _grabbedPos = pos;
    }

    private void SetDirection(Direction dir) {
        _room = _room with { Direction = dir };
        _tile.RotationDegrees = dir.ToDegrees();
    }

    protected override void OnMouseUp(MouseButton button, Vector2 pos) {
        _grabbedPos = null;
    }

    public override void OnUpdate(Vector2 pos, InputSnapshot snapshot) {
        if (_grabbedPos.HasValue && _grabbedPos != pos) {
            var dist = Vector2.Normalize(pos - _grabbedPos.Value);
            var deg = MathF.Acos(Vector2.Dot(dist, Vector2.UnitY)) / MathF.PI * 180;
            SetDirection(deg <= 45 ? Direction.Down :
                deg >= 180 - 45 ? Direction.Up :
                dist.X < 0 ? Direction.Left : Direction.Right);
        }
    }

    public MapTile(GraphicsResources gfxRes, float size) : base(new() { PixelSize = new(size) }) {
        _gfxRes = gfxRes;
        _border = new(gfxRes, new(size), 1, Color.White);
        _internalChildren.Add(_border);
    }
}
