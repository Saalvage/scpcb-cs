using System.Numerics;
using System.Text.Json;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.UserInterface;
using SCPCB.Graphics.UserInterface.Composites;
using SCPCB.Map;
using Veldrid;

namespace SCPCB.Scenes;

public class MapCreatorScene : BaseScene {
    private readonly Game _game;

    private readonly MapGrid _grid;

    public MapCreatorScene(Game game) {
        _game = game;
        var gfx = game.GraphicsResources;
        var input = game.InputManager;

        input.SetMouseCaptured(false);

        var ui = new UIManager(gfx, input);
        AddEntity(ui);

        _grid = new(gfx, 10);
        ui.Root.AddChild(_grid);

        using var roomsFile = File.OpenRead("Assets/Rooms/rooms.json");
        var rooms = JsonSerializer.Deserialize<RoomInfo[]>(roomsFile);
        var roomsDic = rooms.ToDictionary(x => x.Name, x => x);

        var yOff = 0;
        foreach (var room in rooms) {
            var button = new Button(gfx, ui, room.Name, 0, 0, 0, 20) {
                PixelSize = new(128, 32),
                Position = new(_grid.PixelSize.X + 5, yOff += 34),
            };
            button.OnClicked += () => _grid.PlacingRoom = room;
            ui.Root.AddChild(button);
        }

        gfx.GraphicsDevice.SyncToVerticalBlank = true;

        gfx.ShaderCache.SetGlobal<IProjectionMatrixConstantMember, Matrix4x4>(
            Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 180 * 90, (float)gfx.Window.Width / gfx.Window.Height, 0.1f, 100f));

        gfx.ShaderCache.SetGlobal<IUIProjectionMatrixConstantMember, Matrix4x4>(
            Matrix4x4.CreateOrthographic(gfx.Window.Width, gfx.Window.Height, -100, 100));

        foreach (var i in Enumerable.Range(0, 5)) {
            foreach (var j in Enumerable.Range(0, 10)) {
                _grid[i, j] = new(roomsDic[
                    i == 0 || i == 4 || j == 0 || j == 9 ? "room008" :
                    i == 2 || j == 5 ? "coffin" : "4tunnels"], (Direction)((i + j) % 4));
            }
        }
    }

    public override void Update(float delta) {
        base.Update(delta);

        if (_game.InputManager.IsKeyDown(Key.X)) {
            _game.Scene = new MainScene(_game, _grid.GetRooms());
        }
    }
}
