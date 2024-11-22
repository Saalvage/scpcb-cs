using scpcb.Graphics;
using scpcb.Graphics.Primitives;
using scpcb.Scenes;
using scpcb.Utility;

namespace scpcb.Entities.Items;

public class Document : Item {
    private readonly ICBTexture _texture;

    public Document(GraphicsResources gfxRes, IScene scene, Transform transform, string invIcon, string model, string texture)
        : base(gfxRes, scene, invIcon, model, transform) {
        _texture = gfxRes.TextureCache.GetTexture(texture);
    }

    public override void OnUsed() {
        _scene.GetEntitiesOfType<HUD>().Single().SetItem(_texture);
        // TODO: Ugly cast!
        ((MainScene)_scene).SetOpenMenu(null);
    }
}
