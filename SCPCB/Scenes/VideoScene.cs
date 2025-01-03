﻿using System.Numerics;
using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.Textures;
using Veldrid;

namespace SCPCB.Scenes;

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
        _model = new CBModel<UIShader.Vertex>(null, gfxRes.MaterialCache.GetMaterial<UIShader, UIShader.Vertex>(
                [video.Texture], [gfxRes.GraphicsDevice.Aniso4xSampler]),
            new CBMesh<UIShader.Vertex>(gfxRes.GraphicsDevice, [
                    new(new(1, -1)),
                    new(new(-1, -1)),
                    new(new(1, 1)),
                    new(new(-1, 1)),
                ], [2, 1, 0, 3, 1, 2]));

        gfxRes.ShaderCache.SetGlobal<IUIProjectionMatrixConstantMember, Matrix4x4>(
            Matrix4x4.CreateOrthographic(2, 2, -100f, 100f));
        var constants = gfxRes.ShaderCache.GetShader<UIShader>().Constants!;
        constants.SetValue<IPositionConstantMember, Vector3>(Vector3.Zero);
        constants.SetValue<ITexCoordsConstantMember, Vector4>(new(1, 0, 1, 0));
        constants.SetValue<IUIScaleConstantMember, Vector2>(Vector2.One);

        // TODO: Consider if it's worth it to instead create an individual game loop for this scene
        // that only renders when a new frame should be displayed (probably doesn't matter).

        // If we don't do this graphics cards WILL explode.
        _vsyncOld = gfxRes.GraphicsDevice.SyncToVerticalBlank;
        gfxRes.GraphicsDevice.SyncToVerticalBlank = true;

        gfxRes.Window.KeyDown += HandleKeyDown;
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
