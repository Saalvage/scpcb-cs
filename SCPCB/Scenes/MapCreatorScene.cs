using System.Numerics;
using System.Text.Json;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.UserInterface;
using SCPCB.Graphics.UserInterface.Composites;
using SCPCB.Graphics.UserInterface.Utility;
using SCPCB.Map;

namespace SCPCB.Scenes;

public class MapCreatorScene : BaseScene {
    private readonly MapGrid _grid;
    private readonly Game _game;

    public MapCreatorScene(Game game) {
        _game = game;
        var gfx = game.GraphicsResources;
        var input = game.InputManager;

        input.SetMouseCaptured(false);

        var ui = new UIManager(gfx, input);
        AddEntity(ui);

        _grid = new(gfx, ui, 17, 17);
        ui.Root.AddChild(_grid);

        using var roomsFile = File.OpenRead("Assets/Rooms/rooms.json");
        var rooms = JsonSerializer.Deserialize<RoomInfo[]>(roomsFile);
        var roomsDic = rooms.GroupBy(x => x.Shape).ToDictionary(x => x.Key, x => x.ToArray());

        var seed = new InputBox(gfx, ui, input, gfx.FontCache.GetFont("Assets/Fonts/Courier New.ttf", 32)) {
            Alignment = Alignment.BottomLeft,
            Position = new(0, -100),
        };
        seed.Input.OnTextChanged += _ => GenerateMap(seed.Input.Inner.Text);
        ui.Root.AddChild(seed);

        var regenerate = new Button(gfx, ui, "I'm feeling lucky!", 0, 0, 0) {
            Alignment = Alignment.BottomLeft,
        };
        regenerate.OnClicked += () => {
            var newSeed = MapGenerator.GenerateRandomSeed();
            // TODO: This throws in the TextInput when the new seed is shorter and the old one is selected.
            seed.Input.Inner.Text = newSeed;
            GenerateMap(newSeed);
        };
        ui.Root.AddChild(regenerate);

        var width = new InputBox(gfx, ui, input, gfx.FontCache.GetFont("Assets/Fonts/Courier New.ttf", 32)) {
            Alignment = Alignment.BottomRight,
            Position = new(0, -50),
        };
        width.Input.Inner.Text = _grid.Width.ToString();
        width.Input.OnTextChanged += _ => _grid.Width = int.Parse(width.Input.Inner.Text) + 2;
        ui.Root.AddChild(width);

        var height = new InputBox(gfx, ui, input, gfx.FontCache.GetFont("Assets/Fonts/Courier New.ttf", 32)) {
            Alignment = Alignment.BottomRight,
        };
        height.Input.Inner.Text = _grid.Height.ToString();
        height.Input.OnTextChanged += _ => _grid.Height = int.Parse(height.Input.Inner.Text) + 2;
        ui.Root.AddChild(height);

        var start = new Button(gfx, ui, "START", 12.222f, -42, float.E) {
            Alignment = Alignment.BottomRight,
            Position = new(0, -150),
        };
        start.OnClicked += ()
        => {
            var gen = new MapGenerator(_grid.Width - 2, _grid.Height - 2);
            for (var i = 0; i < 1000; i++) {
                gen.GenerateMap(roomsDic, MapGenerator.GenerateRandomSeed());
            }
        };
        ui.Root.AddChild(start);

        var yOff = 0;
        foreach (var room in rooms) {
            var button = new Button(gfx, ui, room.Name, 0, 0, 0, 19) {
                PixelSize = new(128, 32),
                Position = new(_grid.PixelSize.X + 5, yOff += 34),
            };
            button.OnClicked += () => _grid.PlacingRoom = room;
            ui.Root.AddChild(button);
        }

        // TODO: Implement a better system on the scene level.
        gfx.ShaderCache.SetGlobal<IProjectionMatrixConstantMember, Matrix4x4>(
            Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 180 * 90, (float)gfx.Window.Width / gfx.Window.Height, 0.1f, 100f));

        gfx.ShaderCache.SetGlobal<IUIProjectionMatrixConstantMember, Matrix4x4>(
            Matrix4x4.CreateOrthographic(gfx.Window.Width, gfx.Window.Height, -100, 100));

        foreach (var i in Enumerable.Range(0, 5)) {
            foreach (var j in Enumerable.Range(0, 10)) {
                var roomName =
                    i == 0 || i == 4 || j == 0 || j == 9 ? "room008" :
                    i == 2 || j == 5 ? "coffin" : "4tunnels";
                _grid[i, j] = new(rooms.First(x => x.Name == roomName), (Direction)((i + j) % 4));
            }
        }

        void GenerateMap(string seed) {
            var map = new MapGenerator(_grid.Width - 2, _grid.Height - 2).GenerateMap(roomsDic, seed);

            foreach (var (x, y) in Enumerable.Range(0, map.GetLength(0))
                         .SelectMany(x => Enumerable.Range(0, map.GetLength(1)).Select(y => (x, y)))) {
                _grid[x, y] = map[x, y];
            }
        }
    }
}
