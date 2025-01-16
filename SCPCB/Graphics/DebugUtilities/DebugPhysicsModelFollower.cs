using System.Numerics;
using SCPCB.Graphics.Models;
using SCPCB.Graphics.ModelTemplates;
using SCPCB.Utility;

namespace SCPCB.Graphics.DebugUtilities;

public class DebugPhysicsModelFollower : Model {
    private readonly PhysicsModel _model;
    private readonly Vector3 _initialScale;

    public DebugPhysicsModelFollower(IModelTemplate template, PhysicsModel model) : base(template) {
        _model = model;
        _initialScale = model.WorldTransform.Scale;
    }

    public override Transform WorldTransform {
        get => _model.WorldTransform;
        set => _model.WorldTransform = value;
    }

    public override Matrix4x4 GetValue(float interp) {
        return Matrix4x4.CreateScale(Vector3.One / _initialScale)
               * _model.GetInterpolatedTransform(interp).GetMatrix();
    }
}
