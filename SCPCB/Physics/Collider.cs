using BepuPhysics.Collidables;
using SCPCB.Entities;
using SCPCB.Graphics.DebugUtilities;
using SCPCB.Physics.Primitives;
using SCPCB.Scenes;
using SCPCB.Utility;
using System.Numerics;

namespace SCPCB.Physics;

public class Collider : ITickable {
    private readonly IScene _scene;
    private readonly PhysicsResources _physics;

    public readonly CBBody Body;

    //
    // The player controller is shaped like a corn dog.
    //
    //   c - Camera  x - Center of collider
    //
    //     ColliderRadius
    //        ├──────┤
    //           ---           ┬                   ┬
    //          /   \          │ ColliderRadius    │
    //         /     \         ┴                   │
    //        |   c   |        ┬  ┬                │
    //        |       |        │  │ CameraOffset   │ TotalHeight
    //        |   x   |        │  ┴                │  ┬
    //        |       |        │ ColliderLength    │  │
    //        |       |        ┴                   │  │
    //         \     /         ┬                   │  │
    //          \   /          │ ColliderRadius    │  │ HeightOffGround
    //           ---           ┴                   │  │
    //            |            ┬                   │  │
    //            |            │ StepUpHeight      │  │
    //            |            ┴                   ┴  ┴
    // -----------+----------- - Ground level
    //            |            ┬
    //            |            │ StepDownHeight
    //            |            ┴
    //
    public record Dimensions(float ColliderLength, float ColliderRadius, float TotalHeight) {
        public float ColliderTotalHeight => ColliderLength + ColliderRadius * 2;
        public float StepUpHeight => TotalHeight - ColliderTotalHeight;
        public float StepDownHeight => StepUpHeight;
        public float HeightOffGround => ColliderTotalHeight / 2f + StepUpHeight;
        public float CameraOffset { get; init; } = ColliderLength / 2f;
    }
    public Dimensions Info { get; }

    public bool IsFalling { get; set; }

    // TODO: This needs some consideration.
    // 1. Should the stepping velocity be constant? (Right now it effectively decays 1/2 per tick.)
    // 2. Should this be affected by movement speed to prevent "flying off" steps.
    // 3. Should this be represented physically at all? Maybe it could be an effect purely applied to the camera.
    public float SteppingSmoothing { get; set; } = 3f;

    public Collider(IScene scene, Dimensions info) {
        _scene = scene;
        _physics = scene.GetEntitiesOfType<PhysicsResources>().Single();
        Info = info;
        var shape = new Capsule(Info.ColliderRadius, Info.ColliderLength);
        var ret = new CBShape<Capsule>(_physics, shape).CreateDynamic(1);
        ret.Pose = new(Vector3.UnitY * 1.5f + Vector3.UnitX);
        // Never sleep.
        ret.MaySleep = false;
        // Never rotate.
        ret.Inertia = ret.Inertia with { InverseInertiaTensor = default };
        ret.SetProperty<IsInvisibleProperty, bool>(true);
        ret.SetProperty<HasNoFrictionProperty, bool>(true);
        Body = ret;
    }

    public void Tick() {
        // TODO: It would be preferable to cast multiple rays in a circle here and average the floor position for smoother
        // movement over steps. It would still be choppy if not combined with stepping smoothing, but we also want the "snappy"
        // stopping behavior.
        var castLength = Info.HeightOffGround + Info.StepDownHeight;
        var onGround = _physics.RayCast<ClosestRayHitHandler>(Body.Pose.Position, -Vector3.UnitY,
            castLength, x => x.Mobility == CollidableMobility.Static)?.Pos;

        _scene.AddEntity(new DebugLine(_scene.Graphics, TimeSpan.FromSeconds(5),
            Body.Pose.Position - Vector3.UnitY * (0.5f * Info.ColliderTotalHeight),
            Body.Pose.Position - Vector3.UnitY * castLength) {
            Color = onGround != null ? new(0, 1, 0) : new(1, 0, 0),
        });

        // TODO: We probably want to take into account the normal here (don't go up sleep too steep).
        if (onGround != null && (!IsFalling || Vector3.DistanceSquared(Body.Pose.Position, onGround.Value) <= Info.HeightOffGround * Info.HeightOffGround)) {
            var targetPos = onGround.Value + Vector3.UnitY * Info.HeightOffGround;
            // We differentiate because we want to prevent two things:
            if (IsFalling) {
                // 1. When the floating is handled via the velocity then there is always at least one frame
                // where the collider falls "below" the designated height, causing a visual jitter.
                Body.Pose = Body.Pose with { Position = targetPos };
                Body.Velocity = Body.Velocity with { Linear = Body.Velocity.Linear with { Y = 0 } };
            } else {
                // 2. We generally handle floating via the velocity, because this
                // prevents the player from being clipped into the ceiling when stepping.
                Body.Velocity = Body.Velocity with { Linear = Body.Velocity.Linear + Game.TICK_RATE * (targetPos - Body.Pose.Position) / SteppingSmoothing };
            }

            IsFalling = false;
        } else {
            IsFalling = true;
            // TODO: We need to properly unsubscribe the collider from regular pose integration, even though we're handling
            // the velocity completely manually, the integration still has a slight effect.
            Body.Velocity = Body.Velocity with { Linear = Body.Velocity.Linear - Vector3.UnitY * Game.TICK_DELTA };
        }
    }
}
