using OpenTK.Audio.OpenAL;
using SCPCB.Entities;
using SCPCB.Graphics.Textures;
using SCPCB.Utility;
using System.Numerics;

namespace SCPCB.Audio.Properties;

public class DynamicAudioTransformProperty : IAudioProperty, IRenderable, IParentableTransformable {
    public ITransformable? Parent { get; set; }
    public Transform LocalTransform { get; set; }

    public bool IsDirectional {
        get;
        set {
            field = value;
            if (!value) {
                foreach (var i in _playbacks) {
                    AL.Source(i.Handle, ALSource3f.Direction, 0, 0, 0);
                }
            }
        }
    }

    private readonly SourceCollection _playbacks = [];

    public void Apply(Source playback) {
        AL.Source(playback.Handle, ALSourceb.SourceRelative, false);
        _playbacks.Add(playback);
        // TODO: The actual properties are only set during the next render call.
    }

    public void Render(IRenderTarget target, float interp) {
        var trans = ((IParentableTransformable)this).GetInterpolatedWorldTransform(interp);
        var dir = IsDirectional ? Vector3.Transform(Vector3.UnitZ, trans.Rotation) : default;
        Vector3 vel = default;
        if (interp != 0) {
            // TODO: This is hacky and potentially expensive, I can't reasonably imagine an implementation not relying on interpolation
            // between a previous and current transform, so maybe there's a better way to express this.
            var startTrans = ((IParentableTransformable)this).GetInterpolatedWorldTransform(0f);
            vel = (trans.Position - startTrans.Position) / interp;
        }

        foreach (var p in _playbacks) {
            AL.Source(p.Handle, ALSource3f.Position, trans.Position.X, trans.Position.Y, trans.Position.Z);
            if (IsDirectional) {
                AL.Source(p.Handle, ALSource3f.Direction, dir.X, dir.Y, dir.Z);
                AL.Source(p.Handle, ALSourcef.ConeOuterAngle, 180);
                AL.Source(p.Handle, ALSourcef.ConeInnerAngle, 90);
            }
            if (interp != 0) {
                AL.Source(p.Handle, ALSource3f.Velocity, vel.X, vel.Y, vel.Z);
            }
        }
    }
}
