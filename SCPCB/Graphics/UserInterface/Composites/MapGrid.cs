using System.Drawing;
using System.Numerics;
using SCPCB.Graphics.UserInterface.Primitives;
using SCPCB.Graphics.UserInterface.Utility;
using SCPCB.Map;
using Veldrid;

namespace SCPCB.Graphics.UserInterface.Composites;

public class MapGrid : InteractableUIElement<UIElement> {
    private const int TILE_SIZE = 28;
    private const int OFFSET = 1;

    private readonly int _size;
    private readonly int _internalStart;

    private readonly TextureElement _activeMarker;
    private readonly TextureElement _rotator;
    private MapTile? _activeRotator;

    public RoomInfo? PlacingRoom { private get; set; }

    public MapGrid(GraphicsResources gfx, int size) : base(new() { PixelSize = new(TILE_SIZE + (size - 1) * (TILE_SIZE + OFFSET)), }) {
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
        _rotator = new(gfx, gfx.TextureCache.GetTexture("Assets/MapCreator/arrows.png")) {
            Alignment = Alignment.Center,
        };
    }

    public PlacedRoomInfo?[,] GetRooms() {
        var rooms = new PlacedRoomInfo?[_size, _size];
        for (var x = 0; x < _size; x++) {
            for (var y = 0; y < _size; y++) {
                rooms[x, y] = GetMapTile(x, y).Room;
            }
        }
        return rooms;
    }

    public override void OnUpdate(Vector2 pos, InputSnapshot snapshot) {
        var (x, y) = GetIndices(pos);

        if (x >= 0 && x < _size && y >= 0 && y < _size) {
            _activeMarker.Position = new Vector2(x, y) * (TILE_SIZE + OFFSET);
            _activeMarker.IsVisible = true;
        } else {
            _activeMarker.IsVisible = false;
        }
    }

    protected override void OnMouseDown(MouseButton button, Vector2 pos) {
        if (button is not (MouseButton.Left or MouseButton.Right)) {
            return;
        }

        var (x, y) = GetIndices(pos);

        var mapTile = GetMapTile(x, y);
        if (button == MouseButton.Left) {
            if (mapTile.Room != null) {
                mapTile.AddChild(_rotator);
                mapTile.GrabRotate(pos - mapTile.Position);
                _activeRotator = mapTile;
            } else if (PlacingRoom != null) {
                mapTile.Room = new(PlacingRoom, Direction.Up);
            }
        } else {
            mapTile.Room = null;
        }
    }

    protected override void OnMouseUp(MouseButton button, Vector2 pos) {
        _activeRotator?.RemoveChild(_rotator);
        _activeRotator = null;
    }

    private MapTile GetMapTile(int x, int y) => (MapTile)_internalChildren[_internalStart + x * _size + y];

    private (int, int) GetIndices(Vector2 pos) {
        return ((int)(pos.X / (TILE_SIZE + OFFSET)), (int)(pos.Y / (TILE_SIZE + OFFSET)));
    }
}
