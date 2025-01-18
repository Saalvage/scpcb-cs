using System.Numerics;
using SCPCB.Graphics;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.Textures;
using SCPCB.Physics;

namespace SCPCB.Scenes;

public abstract class Scene3D : BaseScene {
    public ICamera Camera { get; protected set; }

    public PhysicsResources Physics { get; }

    protected Scene3D(GraphicsResources gfxRes) : base(gfxRes) {
        Physics = new(Graphics); 

        AddEntity(new SortedRenderer(this, interp => Camera.Position));
    }

    public override void Render(IRenderTarget target, float interp) {
        Graphics.ShaderCache.SetGlobal<IViewMatrixConstantMember, Matrix4x4>(Camera.GetViewMatrix(interp));
        Graphics.ShaderCache.SetGlobal<IViewPositionConstantMember, Vector3>(Camera.Position);

        base.Render(target, interp);
    }
}
