using System.Diagnostics;
using SCPCB.Physics.Primitives;
using SCPCB.Utility;
using System.Numerics;
using SCPCB.Entities;
using SCPCB.Graphics.ModelTemplates;
using SCPCB.Scenes;

namespace SCPCB.Graphics.Models;

public class PhysicsModel : BaseModel, IEntity {
    protected Vector3 _scale = Vector3.One;
    protected Vector3 _offset;

    public CBCollidable Collidable { get; }

    public new IPhysicsModelTemplate Template => (IPhysicsModelTemplate)base.Template;

    public override Transform WorldTransform {
        get => Collidable.Pose.ToTransform() with { Scale = _scale } + new Transform(_offset);
        set {
            if (_scale != value.Scale) {
                Collidable.Shape = Collidable.Shape.GetScaledClone(value.Scale / _scale);
            }

            Collidable.Pose = new(value.Position, value.Rotation);
            _scale = value.Scale;
        }
    }

    public PhysicsModel(IPhysicsModelTemplate template, CBCollidable collidable)
        : base(template) {
        Debug.Assert(template.Shape == collidable.Shape);
        Collidable = collidable;
        _offset = -template.OffsetFromCenter;
    }

    public virtual void OnAdd(IScene scene) {
        Collidable.Attach();
    }

    public virtual void OnRemove(IScene scene) {
        Collidable.Detach();
    }

    protected override void DisposeImpl() {
        Collidable.Dispose();
        base.DisposeImpl();
    }
}
