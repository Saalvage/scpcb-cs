using System.Diagnostics;
using System.Numerics;
using SCPCB.Entities;
using SCPCB.Utility;

namespace SCPCB.Physics.Primitives;

// Small wrapper around a CBBody that offers interpolatable transforms.
public class CBBodyTransformable : ITickable, ITransformable {
    private readonly CBBody _body;

    private Transform _prevTransform;

    public CBBodyTransformable(CBBody body) {
        _body = body;
    }

    public void Tick() {
        _prevTransform = WorldTransform;
    }

    public Transform WorldTransform {
        get => _body.Pose.ToTransform();
        set {
            Debug.Assert(value.Scale == Vector3.One);
            _body.Pose = new(value.Position, value.Rotation);
            _prevTransform = WorldTransform;
        }
    }

    public Transform GetInterpolatedWorldTransform(float interp)
        => Transform.Lerp(_prevTransform, WorldTransform, interp);
}
