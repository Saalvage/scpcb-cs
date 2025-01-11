using BepuPhysics;
using SCPCB.Graphics.Models;
using SCPCB.Physics;

namespace SCPCB.Graphics.ModelTemplates;

public static class ModelTemplateExtensions {
    public static Model Instantiate(this IModelTemplate template) => new(template);

    public static PhysicsModel InstantiatePhysicsStatic(this IPhysicsModelTemplate template, RigidPose pose)
        => new(template, template.Shape.CreateStatic(pose));
    public static DynamicPhysicsModel InstantiatePhysicsDynamic(this IPhysicsModelTemplate template, BodyInertia inertia, BodyActivityDescription activity)
        => new(template, template.Shape.CreateDynamic(RigidPose.Identity, inertia, activity));
    public static DynamicPhysicsModel InstantiatePhysicsKinematic(this IPhysicsModelTemplate template, BodyActivityDescription activity)
        => new(template, template.Shape.CreateKinematic(RigidPose.Identity, activity));
}
