using System.Numerics;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Graphics.UserInterface;
using scpcb.Graphics.UserInterface.Composites;

namespace scpcb.Scenes;

public class MapCreatorScene : BaseScene {
    public MapCreatorScene(Game game) {
        var gfx = game.GraphicsResources;
        var input = game.InputManager;

        input.SetMouseCaptured(false);

        var ui = new UIManager(gfx, input);
        AddEntity(ui);

        ui.Root.AddChild(new MapGrid(gfx, 10));

        gfx.GraphicsDevice.SyncToVerticalBlank = true;

        gfx.ShaderCache.SetGlobal<IProjectionMatrixConstantMember, Matrix4x4>(
            Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 180 * 90, (float)gfx.Window.Width / gfx.Window.Height, 0.1f, 100f));

        gfx.ShaderCache.SetGlobal<IUIProjectionMatrixConstantMember, Matrix4x4>(
            Matrix4x4.CreateOrthographic(gfx.Window.Width, gfx.Window.Height, -100, 100));
    }
}
