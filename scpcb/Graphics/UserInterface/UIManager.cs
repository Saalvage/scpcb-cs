﻿using System.Numerics;
using scpcb.Entities;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders;
using scpcb.Graphics.Textures;

namespace scpcb.Graphics.UserInterface;

public class UIManager : IRenderable {
    public IUIElement Root { get; }

    public int Priority => 200000;

    public ICBMesh<UIShader.Vertex> UIMesh { get; }

    public GraphicsResources GraphicsResources { get; }

    public UIManager(GraphicsResources gfxRes) {
        GraphicsResources = gfxRes;
        UIMesh = new CBMesh<UIShader.Vertex>(gfxRes.GraphicsDevice, [
            new(new(-0.5f, -0.5f), new(0, 1)),
            new(new(0.5f, -0.5f), new(1, 1)),
            new(new(-0.5f, 0.5f), new(0, 0)),
            new(new(0.5f, 0.5f), new(1, 0)),
        ], [0, 1, 2, 3, 2, 1]);
        Root = new UIElement { PixelSize = new(gfxRes.Window.Width, gfxRes.Window.Height) };
    }

    public void Render(IRenderTarget target, float interp) {
        target.ClearDepthStencil();
        Root.Draw(target, Root, Vector2.Zero);
    }
}