using System.Numerics;
using SCPCB.Graphics;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.Textures;
using SCPCB.Physics;

namespace SCPCB.Scenes;

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
        _gfxRes.ShaderCache.SetGlobal<IViewMatrixConstantMember, Matrix4x4>(Camera.ViewMatrix);
        _gfxRes.ShaderCache.SetGlobal<IViewPositionConstantMember, Vector3>(Camera.Position);

        base.Render(target, interp);
    }
}
