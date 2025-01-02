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

    private readonly GraphicsResources _gfx;

    private int _width;
    private int _height;
    public int Width {
        get => _width;
        set {
            if (_width > value) {
                for (var x = _width - 1; x >= value; x--) {
                    for (var y = _height - 1; y >= 0; y--) {
                        _internalChildren.RemoveAt(GetIndex(x, y));
                    }
                }
            } else if (_width < value) {
                for (var x = _width; x < value; x++) {
                    for (var y = 0; y < _height; y++) {
                        _internalChildren.Insert(GetIndex(x, y), MakeTile(x, y));
                    }
                }
            }
            _width = value;
            UpdatePixelSize();
        }
    }
    public int Height {
        get => _height;
        set {
            if (_height > value) {
                for (var x = _width - 1; x >= 0; x--) {
                    for (var y = _height - 1; y >= value; y--) {
                        _internalChildren.RemoveAt(GetIndex(x, y));
                    }
                }
            } else if (_height < value) {
                var oldHeight = _height;
                _height = value;
                for (var x = 0; x < _width; x++) {
                    for (var y = oldHeight; y < value; y++) {
                        _internalChildren.Insert(GetIndex(x, y), MakeTile(x, y));
                    }
                }
            }
            _height = value;
            UpdatePixelSize();
        }
    }

    private readonly int _internalStart;

    private readonly MouseFollowerElement<Label> _infoBox;
    private readonly TextureElement _activeMarker;
    private readonly TextureElement _rotator;
    private MapTile? _activeRotator;

    public RoomInfo? PlacingRoom { private get; set; }

    public MapGrid(GraphicsResources gfx, UIManager ui, int width, int height)
            : base(new()) {
        _gfx = gfx;

        _infoBox = new(new(gfx, ui, 0, 0, 0, 19)) {
            PixelSize = new(128, 32),
            Z = 10,
        };
        var infoText = _infoBox.Inner.Text;
        infoText.Alignment = Alignment.CenterLeft;
        infoText.Position = new(5, 0);
        _internalChildren.Add(_infoBox);

        _internalStart = _internalChildren.Count;
        Width = width;
        Height = height;

        _internalChildren.Add(_activeMarker = new(gfx, gfx.TextureCache.GetTexture(Color.White)) {
            PixelSize = new(TILE_SIZE),
            Color = Color.FromArgb(0xC8, Color.White),
        });
        _rotator = new(gfx, gfx.TextureCache.GetTexture("Assets/MapCreator/arrows.png")) {
            Alignment = Alignment.Center,
        };
    }

    private void UpdatePixelSize() {
        PixelSize = new Vector2(TILE_SIZE) + new Vector2(_width - 1, _height - 1) * (TILE_SIZE + OFFSET);
    }

    public PlacedRoomInfo?[,] GetRooms() {
        var rooms = new PlacedRoomInfo?[_width, _height];
        for (var x = 0; x < _width; x++) {
            for (var y = 0; y < _height; y++) {
                rooms[x, y] = GetMapTile(x, y).Room;
            }
        }
        return rooms;
    }

    public PlacedRoomInfo? this[int x, int y] {
        get => GetMapTile(x, y).Room;
        set => GetMapTile(x, y).Room = value;
    }

    public override void OnUpdate(Vector2 pos, InputSnapshot snapshot) {
        var (x, y) = GetIndices(pos);

        if (x >= 0 && x < _width && y >= 0 && y < _height) {
            _activeMarker.Position = new Vector2(x, y) * (TILE_SIZE + OFFSET);
            _activeMarker.IsVisible = true;

            var room = GetMapTile(x, y).Room;
            if (room == null || _activeRotator != null || snapshot.IsMouseDown(MouseButton.Right)) {
                _infoBox.IsVisible = false;
            } else {
                _infoBox.IsVisible = true;
                _infoBox.Inner.Text.Text = $"Room: {room.Room.Name}\nRotation: {room.Direction.ToDegrees()}°";
            }
        } else {
            _activeMarker.IsVisible = false;
            _infoBox.IsVisible = false;
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
                // If a room can be rotated to fit the surroundings exactly when being placed, do so.
                // This could be improved:
                // - Rotate surrounding rooms if the newly placed one makes them fit exactly.
                // - Rotate rooms of shape 2 if they have one connecting room.
                // There should probably be an option to disable this as well.
                // I'm also not sure if this is even helpful in all that many circumstances.
                Span<(int, int)> offsets = [(0, -1), (1, 0), (0, 1), (-1, 0)];
                Span<bool> connects = stackalloc bool[4];
                foreach (var (dir, i) in Enum.GetValues<Direction>().Select((x, i) => (x, i))) {
                    var (dx, dy) = offsets[i];
                    connects[i] = x + dx >= 0 && x + dx < _width && y + dy >= 0 && y + dy < _height
                                  && GetMapTile(x + dx, y + dy).Room?.HasOpening(dir.Rotate(2)) == true;
                }
                var correctAmount = connects.Count(true) == PlacingRoom.Shape switch {
                    Shape._1 => 1,
                    Shape._2 or Shape._2C => 2,
                    Shape._3 => 3,
                    Shape._4 => 4,
                };
                var finalDir = correctAmount
                    ? PlacingRoom.Shape switch {
                        Shape._1 => ((Direction)connects.IndexOf(true)).Rotate(2),
                        Shape._2 when connects.IndexOf(true) % 2 == connects.LastIndexOf(true) % 2 => (Direction)connects.IndexOf(true),
                        Shape._3 => (Direction)connects.IndexOf(false),
                        // Magic function!
                        Shape._2C => ((Direction)connects.IndexOf(true) + connects.IndexOf(false) % 2 * 3).Rotate(3),
                        _ => Direction.Up,
                    }
                    : Direction.Up;

                mapTile.Room = new(PlacingRoom, finalDir);
            }
        } else {
            mapTile.Room = null;
        }
    }

    protected override void OnMouseUp(MouseButton button, Vector2 pos) {
        _activeRotator?.RemoveChild(_rotator);
        _activeRotator = null;
    }

    private MapTile MakeTile(int x, int y) => new (_gfx, TILE_SIZE) {
        Position = new((TILE_SIZE + OFFSET) * x, (TILE_SIZE + OFFSET) * y),
    };

    private int GetIndex(int x, int y) => _internalStart + x * _height + y;

    private MapTile GetMapTile(int x, int y) => (MapTile)_internalChildren[GetIndex(x, y)];

    private (int, int) GetIndices(Vector2 pos) => ((int)(pos.X / (TILE_SIZE + OFFSET)), (int)(pos.Y / (TILE_SIZE + OFFSET)));
}
