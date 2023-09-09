using scpcb.Graphics;
using scpcb.Graphics.Textures;
using scpcb.Physics;

namespace scpcb.Scenes;

public class Scene3D : BaseScene {
    private readonly GraphicsResources _gfxRes;

    public ICamera Camera { get; protected set; }

    public PhysicsResources Physics { get; }

    public Scene3D(GraphicsResources gfxRes) {
        _gfxRes = gfxRes;
        Physics = new(_gfxRes); 

        AddEntity(new ModelSorter(this, interp => Camera.Position));
    }

    public override void Render(IRenderTarget target, float interp) {
        Camera.ApplyTo(_gfxRes.ShaderCache.ActiveShaders.Select(x => x.Constants), interp);

        base.Render(target, interp);
    }
}
