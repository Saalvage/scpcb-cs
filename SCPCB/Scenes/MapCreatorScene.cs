using System.Numerics;
using System.Text.Json;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.UserInterface;
using SCPCB.Graphics.UserInterface.Composites;
using SCPCB.Graphics.UserInterface.Utility;
using SCPCB.Map;

namespace SCPCB.Scenes;

public class MapCreatorScene : BaseScene {
    public MapCreatorScene(Game game) : base(game.GraphicsResources) {
        var input = game.InputManager;

        input.SetMouseCaptured(false);

        var ui = new UIManager(Graphics, input);
        AddEntity(ui);

        var grid = new MapGrid(Graphics, ui, 17, 17);
        ui.Root.AddChild(grid);

        using var roomsFile = File.OpenRead("Assets/Rooms/rooms.json");
        var rooms = JsonSerializer.Deserialize<RoomInfo[]>(roomsFile);
        var roomsDic = rooms.GroupBy(x => x.Shape).ToDictionary(x => x.Key, x => x.ToArray());

        var seed = new InputBox(Graphics, ui, input, Graphics.FontCache.GetFont("Assets/Fonts/Courier New.ttf", 32)) {
            Alignment = Alignment.BottomLeft,
            Position = new(0, -100),
        };
        seed.Input.OnTextChanged += _ => GenerateMap(seed.Input.Inner.Text);
        ui.Root.AddChild(seed);

        var regenerate = new Button(Graphics, ui, "I'm feeling lucky!", 0, 0, 0) {
            Alignment = Alignment.BottomLeft,
        };
        regenerate.OnClicked += () => {
            var newSeed = MapGenerator.GenerateRandomSeed();
            // TODO: This throws in the TextInput when the new seed is shorter and the old one is selected.
            seed.Input.Inner.Text = newSeed;
            GenerateMap(newSeed);
        };
        ui.Root.AddChild(regenerate);

        var width = new InputBox(Graphics, ui, input, Graphics.FontCache.GetFont("Assets/Fonts/Courier New.ttf", 32)) {
            Alignment = Alignment.BottomRight,
            Position = new(0, -50),
        };
        width.Input.Inner.Text = grid.Width.ToString();
        width.Input.OnTextChanged += _ => grid.Width = int.Parse(width.Input.Inner.Text);
        ui.Root.AddChild(width);

        var height = new InputBox(Graphics, ui, input, Graphics.FontCache.GetFont("Assets/Fonts/Courier New.ttf", 32)) {
            Alignment = Alignment.BottomRight,
        };
        height.Input.Inner.Text = grid.Height.ToString();
        height.Input.OnTextChanged += _ => grid.Height = int.Parse(height.Input.Inner.Text);
        ui.Root.AddChild(height);

        var start = new Button(Graphics, ui, "START", 12.222f, -42, float.E) {
            Alignment = Alignment.BottomRight,
            Position = new(0, -150),
        };
        start.OnClicked += () => game.Scene = new MainScene(game, grid.GetRooms());
        ui.Root.AddChild(start);

        var yOff = 0;
        foreach (var room in rooms) {
            var button = new Button(Graphics, ui, room.Name, 0, 0, 0, 19) {
                PixelSize = new(128, 32),
                Position = new(grid.PixelSize.X + 5, yOff += 34),
            };
            button.OnClicked += () => grid.PlacingRoom = room;
            ui.Root.AddChild(button);
        }

        // TODO: Implement a better system on the scene level.
        Graphics.ShaderCache.SetGlobal<IProjectionMatrixConstantMember, Matrix4x4>(
            Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 180 * 90, (float)Graphics.Window.Width / Graphics.Window.Height, 0.1f, 100f));

        Graphics.ShaderCache.SetGlobal<IUIProjectionMatrixConstantMember, Matrix4x4>(
            Matrix4x4.CreateOrthographic(Graphics.Window.Width, Graphics.Window.Height, -100, 100));

        foreach (var i in Enumerable.Range(0, 5)) {
            foreach (var j in Enumerable.Range(0, 10)) {
                var roomName =
                    i == 0 || i == 4 || j == 0 || j == 9 ? "room008" :
                    i == 2 || j == 5 ? "coffin" : "4tunnels";
                grid[i, j] = new(rooms.First(x => x.Name == roomName), (Direction)((i + j) % 4));
            }
        }

        void GenerateMap(string seed) {
            var map = new MapGenerator(grid.Width, grid.Height).GenerateMap(roomsDic, seed);

            foreach (var (x, y) in Enumerable.Range(0, map.GetLength(0))
                         .SelectMany(x => Enumerable.Range(0, map.GetLength(1)).Select(y => (x, y)))) {
                grid[x, y] = map[x, y];
            }
        }
    }
}
