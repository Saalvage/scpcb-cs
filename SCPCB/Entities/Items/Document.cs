using SCPCB.Graphics;
using SCPCB.Graphics.Primitives;
using SCPCB.Scenes;
using SCPCB.Utility;

namespace SCPCB.Entities.Items;

public class Document : Item {
    private readonly ICBTexture _texture;

    public Document(GraphicsResources gfxRes, IScene scene, Transform transform, float scale, string invIcon, string model, string texture)
        : base(gfxRes, scene, invIcon, model, transform with { Scale = transform.Scale * scale }) {
        _texture = gfxRes.TextureCache.GetTexture(texture);
    }

    public override void OnUsed() {
        _scene.GetEntitiesOfType<HUD>().Single().SetItem(_texture);
        // TODO: Ugly cast!
        ((MainScene)_scene).SetOpenMenu(null);
    }
}