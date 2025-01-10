using System.Numerics;
using SCPCB.Graphics;
using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.Textures;
using Veldrid;

namespace SCPCB.Scenes;

public class VideoScene : BaseScene {
    private readonly IMeshInstance _model;
    private readonly bool _vsyncOld;

    private readonly Game _game;

    public VideoScene(Game game, string filename) : base(game.GraphicsResources) {
        _game = game;

        var video = new Video(Graphics, filename);
        video.Finished += MoveToMain;

        AddEntity(video);
        _model = new MeshInstance<UIShader.Vertex>(null, Graphics.MaterialCache.GetMaterial<UIShader, UIShader.Vertex>(
                [video.Texture], [Graphics.GraphicsDevice.Aniso4xSampler]),
            new CBMesh<UIShader.Vertex>(Graphics.GraphicsDevice, [
                    new(new(1, -1)),
                    new(new(-1, -1)),
                    new(new(1, 1)),
                    new(new(-1, 1)),
                ], [2, 1, 0, 3, 1, 2]));

        Graphics.ShaderCache.SetGlobal<IUIProjectionMatrixConstantMember, Matrix4x4>(
            Matrix4x4.CreateOrthographic(2, 2, -100f, 100f));
        var constants = Graphics.ShaderCache.GetShader<UIShader>().Constants!;
        constants.SetValue<IPositionConstantMember, Vector3>(Vector3.Zero);
        constants.SetValue<ITexCoordsConstantMember, Vector4>(new(1, 0, 1, 0));
        constants.SetValue<IUIScaleConstantMember, Vector2>(Vector2.One);

        // TODO: Consider if it's worth it to instead create an individual game loop for this scene
        // that only renders when a new frame should be displayed (probably doesn't matter).

        // If we don't do this graphics cards WILL explode.
        _vsyncOld = Graphics.GraphicsDevice.SyncToVerticalBlank;
        Graphics.GraphicsDevice.SyncToVerticalBlank = true;

        Graphics.Window.KeyDown += HandleKeyDown;
    }

    public override void Render(IRenderTarget target, float interp) {
        _model.Render(target, interp);
    }

    public override void OnLeave() {
        _game.GraphicsResources.Window.KeyDown -= HandleKeyDown;
        _game.GraphicsResources.GraphicsDevice.SyncToVerticalBlank = _vsyncOld;
    }

    private void HandleKeyDown(KeyEvent e) {
        if (e.Key == Key.Space) {
            MoveToMain();
        }
    }

    private bool _moved = false;

    private void MoveToMain() {
        if (!_moved) {
            _moved = true;
            _game.Scene = new MainScene(_game);
        }
    }

    protected override void DisposeImpl() {
        _model.Mesh.Dispose();
        base.DisposeImpl();
    }
}
