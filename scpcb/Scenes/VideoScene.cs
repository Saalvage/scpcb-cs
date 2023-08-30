using scpcb.Graphics;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders;
using scpcb.Utility;
using Veldrid;

namespace scpcb.Scenes; 

public class VideoScene : BaseScene {
    private readonly ICBModel _model;
    private readonly bool _vsyncOld;

    private readonly Game _game;

    public VideoScene(Game game, string filename) {
        _game = game;

        var gfxRes = game.GraphicsResources;

        var video = new Video(gfxRes, filename);
        video.Finished += MoveToMain;

        AddEntity(video);
        _model = new CBModel<UIShader.Vertex>(null, gfxRes.ShaderCache.GetShader<UIShader, UIShader.Vertex>()
                .CreateMaterial(video.Texture.AsEnumerableElement(),
                    gfxRes.GraphicsDevice.Aniso4xSampler.AsEnumerableElement()),
            new CBMesh<UIShader.Vertex>(gfxRes.GraphicsDevice,
                new UIShader.Vertex[] {
                    new(new(-1, -1), new(0, 1)),
                    new(new(-1, 1), new(0, 0)),
                    new(new(1, -1), new(1, 1)),
                    new(new(1, 1), new(1, 0)),
                }, new uint[] { 2, 1, 0, 3, 1, 2 }));

        // TODO: Consider if it's worth it to instead create an individual game loop for this scene
        // that only renders when a new frame should be displayed (probably doesn't matter).

        // If we don't do this graphics cards WILL explode.
        _vsyncOld = gfxRes.GraphicsDevice.SyncToVerticalBlank;
        gfxRes.GraphicsDevice.SyncToVerticalBlank = true;

        gfxRes.Window.KeyDown += HandleKeyDown;
    }

    public override void Render(RenderTarget target, float interp) {
        target.Render(_model, interp);
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
        _model.Material.Dispose();
        _model.Mesh.Dispose();
        base.DisposeImpl();
    }
}
