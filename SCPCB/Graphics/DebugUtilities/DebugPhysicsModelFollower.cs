using System.Numerics;
using SCPCB.Graphics.Models;
using SCPCB.Graphics.ModelTemplates;

namespace SCPCB.Graphics.DebugUtilities;

public class DebugPhysicsModelFollower : Model {
    public DebugPhysicsModelFollower(IModelTemplate template, PhysicsModel model) : base(template) {
        Parent = model;
        LocalTransform = new() { Scale = Vector3.One / model.WorldTransform.Scale };
    }
}
