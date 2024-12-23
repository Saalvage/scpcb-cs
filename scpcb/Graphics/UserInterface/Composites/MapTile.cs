using System.Drawing;
using System.Numerics;
using scpcb.Graphics.UserInterface.Primitives;
using Veldrid;

namespace scpcb.Graphics.UserInterface.Composites;

public enum Direction {
    Up,
    Down,
    Left,
    Right,
}

public static class DirectionExtensions {
    public static float ToDegrees(this Direction dir) => dir switch {
        Direction.Up => 0,
        Direction.Down => 180,
        Direction.Left => 90,
        Direction.Right => -90,
    };
}

public class MapTile : InteractableUIElement<UIElement> {
    private readonly Border _border;
    private TextureElement? _tile;

    private Vector2? _grabbedPos;

    public TextureElement? Tile {
        get => _tile;
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

    private Direction _direction;

    public Direction Direction {
        get => _direction;
        private set {
            _direction = value;
            _tile.RotationDegrees = value.ToDegrees();
        }
    }

    public void GrabRotate(Vector2 pos) {
        _grabbedPos = pos;
    }

    protected override void OnMouseUp(MouseButton button, Vector2 pos) {
        _grabbedPos = null;
    }

    public override void OnUpdate(Vector2 pos, InputSnapshot snapshot) {
        if (_grabbedPos.HasValue && _grabbedPos != pos) {
            var dist = Vector2.Normalize(pos - _grabbedPos.Value);
            var deg = MathF.Acos(Vector2.Dot(dist, Vector2.UnitY)) / MathF.PI * 180;
            Direction = deg <= 45 ? Direction.Down :
                deg >= 180 - 45 ? Direction.Up :
                dist.X < 0 ? Direction.Left : Direction.Right;
        }
    }

    public MapTile(GraphicsResources gfxResRes, float size) : base(new() { PixelSize = new(size) }) {
        _border = new(gfxResRes, new(size), 1, Color.White);
        _internalChildren.Add(_border);
    }
}
