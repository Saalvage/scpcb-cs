﻿using System.Numerics;
using scpcb.Entities;
using scpcb.Graphics;
using scpcb.Graphics.Primitives;
using scpcb.Scenes;
using scpcb.Utility;

namespace scpcb.Map.Entities;

public class Screen : IMapEntity, IPickableEntity {
    public Vector3 Position { get; }

    private readonly ICBTexture _handTexture;
    private readonly ICBTexture _screenTexture;

    private readonly IScene _scene;

    public Screen(GraphicsResources gfxRes, IScene scene, Transform transform, string imgPath) {
        // TODO: Maybe a centralized location to store the hand textures instead of having each screen store a reference?
        _handTexture = gfxRes.TextureCache.GetTexture("Assets/Textures/handsymbol.png");
        _screenTexture = gfxRes.TextureCache.GetTexture("Assets/Textures/Screens/" + imgPath);

        _scene = scene;

        Position = transform.Position;
    }

    public void OnPicked(IPlayer player) {
        _scene.GetEntitiesOfType<HUD>().Single().SetItem(_screenTexture);
    }

    public ICBTexture GetHandTexture() => _handTexture;
}