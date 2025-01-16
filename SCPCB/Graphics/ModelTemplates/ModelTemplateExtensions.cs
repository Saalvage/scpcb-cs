using BepuPhysics;
using SCPCB.Graphics.Models;
using SCPCB.Physics;
using SCPCB.Physics.Primitives;

namespace SCPCB.Graphics.ModelTemplates;

public static class ModelTemplateExtensions {
    public static Model Instantiate(this IModelTemplate template) => new(template);

    public static PhysicsModel InstantiatePhysicsStatic(this IPhysicsModelTemplate template)
        => new(template, template.Shape.CreateStatic());

    public static PhysicsModel InstantiatePhysicsStatic(this IPhysicsModelTemplate template, RigidPose pose)
        => new(template, template.Shape.CreateStatic(pose));

    public static DynamicPhysicsModel InstantiatePhysicsDynamic(this IPhysicsModelTemplate template, float mass) {
        // TODO: I really fucking hate this. The information whether a shape is convex pervades through the hierarchy bottom-up
        // and I feel like stopping this here seems reasonable.
        if (!template.Shape.IsConvex) {
            throw new ArgumentException("To instantiate a physics model template with a mass its shape must be convex.", nameof(template));
        }

        var bodyType = typeof(CBConvexBody<>).MakeGenericType(template.Shape.Shape.GetType());
        var body = (CBBody)bodyType.GetConstructors().Single().Invoke([template.Shape, mass]);
        return new(template, body);
    }
}
