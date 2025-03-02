using SCPCB.Graphics.ModelTemplates;
using SCPCB.Physics.Primitives;
using SCPCB.Scenes;
using SCPCB.Utility;

namespace SCPCB.Graphics.Models;

public class DynamicPhysicsModel : PhysicsModel {
    // TODO: This should probably be moved down the hierarchy or become some sort of customization point.
    private Transform _previousWorldTransform;

    public CBBody Body => (CBBody)Collidable;

    public override Transform WorldTransform {
        get => base.WorldTransform;
        set {
            base.WorldTransform = value;
            _previousWorldTransform = value;
        }
    }

    public DynamicPhysicsModel(IPhysicsModelTemplate template, CBBody body)
        : base(template, body) { }

    /// <summary>
    /// Transform while maintaining interpolation to previous position
    /// </summary>
    /// <remarks>
    /// This is the opposite of "teleportation", which is how just setting the world transform behaves.
    /// It's designed like this because teleportation is the more common use case.
    /// </remarks>
    public void TransformSmooth(Transform to) {
        base.WorldTransform = to;
    }

    public override void OnAdd(IScene scene) {
        base.OnAdd(scene);
        Body.Physics.AfterUpdate += UpdateTransform;
    }

    public override void OnRemove(IScene scene) {
        Body.Physics.AfterUpdate -= UpdateTransform;
        base.OnRemove(scene);
    }

    private void UpdateTransform() {
        _previousWorldTransform = WorldTransform;
    }

    public override Transform GetInterpolatedWorldTransform(float interp)
        => Transform.Lerp(_previousWorldTransform, WorldTransform, interp);
}
