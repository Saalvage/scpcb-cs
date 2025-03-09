using System.Numerics;
using OpenTK.Audio.OpenAL;
using SCPCB.Entities;
using SCPCB.Graphics.Textures;
using SCPCB.Utility;

namespace SCPCB.Audio;

public class AudioListener : ISingletonEntity<AudioListener>, IRenderable, IParentableTransformable {
    public ITransformable? Parent { get; set; }
    public Transform LocalTransform { get; set; } = new();

    public unsafe void Render(IRenderTarget target, float interp) {
        var trans = ((IParentableTransformable)this).GetInterpolatedWorldTransform(interp);
        AL.Listener(ALListener3f.Position, trans.Position.X, trans.Position.Y, trans.Position.Z);
        var forward = Vector3.Transform(Vector3.UnitZ, trans.Rotation);
        var up = Vector3.Transform(Vector3.UnitY, trans.Rotation);
        Span<float> orientation = [
            forward.X, forward.Y, forward.Z,
            up.X, up.Y, up.Z,
        ];
        fixed (float* ptr = orientation) {
            AL.Listener(ALListenerfv.Orientation, ptr);
        }
        var startTrans = ((IParentableTransformable)this).GetInterpolatedWorldTransform(0f);
        if (interp != 0) {
            var vel = (trans.Position - startTrans.Position) / interp;
            AL.Listener(ALListener3f.Velocity, vel.X, vel.Y, vel.Z);
        }
    }
}
