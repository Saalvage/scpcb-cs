using System.Numerics;
using OpenTK.Audio.OpenAL;
using SCPCB.Entities;
using SCPCB.Graphics.Textures;
using SCPCB.Utility;

namespace SCPCB.Audio;

public class AudioChannel3D : AudioChannel, IParentableTransformable, IRenderable {
    public ITransformable? Parent { get; set; }
    public Transform LocalTransform { get; set; } = new();

    public bool IsDirectional {
        get;
        set {
            field = value;
            if (!value) {
                AL.Source(_source, ALSource3f.Direction, 0, 0, 0);
            }
        }
    }

    public AudioChannel3D() : base(true) { }

    public void Render(IRenderTarget target, float interp) {
        var trans = ((IParentableTransformable)this).GetInterpolatedWorldTransform(interp);
        AL.Source(_source, ALSource3f.Position, trans.Position.X, trans.Position.Y, trans.Position.Z);
        if (IsDirectional) {
            var dir = Vector3.Transform(Vector3.UnitZ, trans.Rotation);
            AL.Source(_source, ALSource3f.Direction, dir.X, dir.Y, dir.Z);
            AL.Source(_source, ALSourcef.ConeOuterAngle, 180);
            AL.Source(_source, ALSourcef.ConeInnerAngle, 90);
        }
        // TODO: This is hacky and potentially expensive, I can't reasonably imagine an implementation not relying on interpolation
        // between a previous and current transform, so maybe there's a better way to express this.
        var startTrans = ((IParentableTransformable)this).GetInterpolatedWorldTransform(0f);
        if (interp != 0) {
            var vel = (trans.Position - startTrans.Position) / interp;
            AL.Source(_source, ALSource3f.Velocity, vel.X, vel.Y, vel.Z);
        }
    }
}
