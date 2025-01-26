using System.Diagnostics;
using System.Numerics;
using BepuPhysics.Collidables;
using SCPCB.Physics;
using SCPCB.Physics.Primitives;
using SCPCB.Utility;
using Helpers = SCPCB.Utility.Helpers;
using SCPCB.Graphics.DebugUtilities;

namespace SCPCB.PlayerController;

public partial class Player {
    private bool _noclip;
    public bool Noclip {
        get => _noclip;
        set {
            if (_noclip == value) {
                return;
            }

            if (value) {
                _collider.Detach();
            } else {
                _collider.Attach();
                _collider.Pose = _collider.Pose with { Position = Camera.Position - Vector3.UnitY * CAMERA_OFFSET };
                // Prevent being sucked onto the ground.
                IsFalling = true;
            }
            _noclip = value;
        }
    }

    public bool IsFalling { get; private set; }
    public bool IsSprinting { get; set; } = false;
    public float BaseSpeed { get; } = 2;

    public Vector2 MoveDir { get; set; } = Vector2.Zero;

    private Vector3 _noclipVelocity;
    public Vector3 Velocity => Noclip ? _noclipVelocity : _collider.Velocity.Linear;

    public bool HasInfiniteStamina { get; set; } = true;
    public float Stamina { get; private set; } = 20f;
    public float MaxStamina { get; } = 100f;

    private readonly CBBody _collider;

    //
    // The player controller is shaped like a corn dog.
    //
    //   c - Camera  x - Center of collider
    //
    //      COLLIDER_RADIUS
    //        ├──────┤
    //           ---           ┬                   ┬
    //          /   \          │ COLLIDER_RADIUS   │
    //         /     \         ┴                   │
    //        |   c   |        ┬  ┬                │
    //        |       |        │  │ CAMERA_OFFSET  │ TOTAL_HEIGHT
    //        |   x   |        │  ┴                │  ┬
    //        |       |        │ COLLIDER_LENGTH   │  │
    //        |       |        ┴                   │  │
    //         \     /         ┬                   │  │
    //          \   /          │ COLLIDER_RADIUS   │  │ HEIGHT_OFF_GROUND
    //           ---           ┴                   │  │
    //            |            ┬                   │  │
    //            |            │ STEP_UP_HEIGHT    │  │
    //            |            ┴                   ┴  ┴
    // -----------+----------- - Ground level
    //            |            ┬
    //            |            │ STEP_DOWN_HEIGHT
    //            |            ┴
    //
    public const float COLLIDER_LENGTH = 1.5f;
    public const float COLLIDER_RADIUS = 0.25f;
    public const float COLLIDER_TOTAL_HEIGHT = COLLIDER_LENGTH + COLLIDER_RADIUS * 2;
    public const float TOTAL_HEIGHT = 2.5f;
    public const float STEP_UP_HEIGHT = TOTAL_HEIGHT - COLLIDER_TOTAL_HEIGHT;
    public const float STEP_DOWN_HEIGHT = STEP_UP_HEIGHT;
    public const float HEIGHT_OFF_GROUND = COLLIDER_TOTAL_HEIGHT / 2f + STEP_UP_HEIGHT;
    public const float CAMERA_OFFSET = COLLIDER_LENGTH / 2f;

    // TODO: This needs some consideration.
    // 1. Should the stepping velocity be constant? (Right now it effectively decays 1/2 per tick.)
    // 2. Should this be affected by movement speed to prevent "flying off" steps.
    // 3. Should this be represented physically at all? Maybe it could be an effect purely applied to the camera.
    public float SteppingSmoothing { get; set; } = 3f;

    private CBBody CreateCollider(PhysicsResources physics) {
        var shape = new Capsule(COLLIDER_RADIUS, COLLIDER_LENGTH);
        var ret = new CBShape<Capsule>(physics, shape).CreateDynamic(1);
        ret.Pose = new(Vector3.UnitY * 1.5f + Vector3.UnitX);
        // Never sleep.
        ret.MaySleep = false;
        // Never rotate.
        ret.Inertia = ret.Inertia with { InverseInertiaTensor = default };
        ret.SetProperty<IsInvisibleProperty, bool>(true);
        ret.SetProperty<HasNoFrictionProperty, bool>(true);
        return ret;
    }

    private void UpdateMovement(float delta) {
        Camera.UpdatePosition();
        var fpsFactor = delta * Helpers.DELTA_TO_FPS_FACTOR_FACTOR;

        Debug.Assert(MoveDir.LengthSquared() <= 1f);

        if (HasInfiniteStamina) {
            Stamina += MathF.Min(MaxStamina, (MaxStamina - Stamina) * 0.01f * fpsFactor);
        }

        var speed = delta * (IsSprinting && Stamina > 0 ? 2.5f : 1f) * BaseSpeed;
        if (MoveDir != Vector2.Zero) {
            if (IsSprinting) {
                // TODO: Consider StaminaEffect.
                Stamina -= fpsFactor * 0.4f;
                if (Stamina <= 0) {
                    Stamina = -20;
                }
            } else {
                Stamina += fpsFactor * 0.15f / 1.25f;
            }
        } else {
            Stamina += fpsFactor * 0.15f * 1.25f;
        }
        Stamina = Math.Min(Stamina, MaxStamina);

        if (Noclip) {
            var dir = Vector3.Transform(new(MoveDir.X, 0, MoveDir.Y), Camera.Rotation);
            var vel = dir * speed * 5;
            _noclipVelocity = vel * Game.TICK_RATE;
            Camera.Position += vel;
        } else {
            if (MoveDir == Vector2.Zero) {
                // TODO: Proper falling physics.
                if (!IsFalling) {
                    _collider.Velocity = Vector3.Zero;
                }
            } else {
                var forward = Vector3.Transform(Vector3.UnitZ, Camera.Rotation);
                forward.Y = 0;
                var right = Vector3.Cross(forward, Vector3.UnitY);
                var dir = Vector3.Normalize(forward) * MoveDir.Y + Vector3.Normalize(right) * -MoveDir.X;
                _collider.Velocity = dir * speed * Game.TICK_RATE;
            }

            // TODO: It would be preferable to cast multiple rays in a circle here and average the floor position for smoother
            // movement over steps. It would still be choppy if not combined with stepping smoothing, but we also want the "snappy"
            // stopping behavior.
            var castLength = HEIGHT_OFF_GROUND + STEP_DOWN_HEIGHT;
            var onGround = _physics.RayCast<ClosestRayHitHandler>(_collider.Pose.Position, -Vector3.UnitY,
                castLength, x => x.Mobility == CollidableMobility.Static)?.Pos;

            _scene.AddEntity(new DebugLine(_scene.Graphics, TimeSpan.FromSeconds(5),
                    _collider.Pose.Position - Vector3.UnitY * (0.5f * COLLIDER_TOTAL_HEIGHT),
                    _collider.Pose.Position - Vector3.UnitY * castLength) {
                Color = onGround != null ? new(0, 1, 0) : new(1, 0, 0),
            });

            // TODO: We probably want to take into account the normal here (don't go up sleep too steep).
            if (onGround != null && (!IsFalling || Vector3.DistanceSquared(_collider.Pose.Position, onGround.Value) <= HEIGHT_OFF_GROUND * HEIGHT_OFF_GROUND)) {
                var targetPos = onGround.Value + Vector3.UnitY * HEIGHT_OFF_GROUND;
                // We differentiate because we want to prevent two things:
                if (IsFalling) {
                    // 1. When the floating is handled via the velocity then there is always at least one frame
                    // where the collider falls "below" the designated height, causing a visual jitter.
                    _collider.Pose = _collider.Pose with { Position = targetPos };
                    _collider.Velocity = _collider.Velocity with { Linear = _collider.Velocity.Linear with { Y = 0 } };
                } else {
                    // 2. We generally handle floating via the velocity, because this
                    // prevents the player from being clipped into the ceiling when stepping.
                    _collider.Velocity = _collider.Velocity with
                        { Linear = _collider.Velocity.Linear + Game.TICK_RATE * (targetPos - _collider.Pose.Position) / SteppingSmoothing };
                }

                IsFalling = false;
            } else {
                IsFalling = true;
                // TODO: We need to properly unsubscribe the collider from regular pose integration, even though we're handling
                // the velocity completely manually, the integration still has a slight effect.
                _collider.Velocity = _collider.Velocity with { Linear = _collider.Velocity.Linear - Vector3.UnitY * delta };
            }
            Camera.Position = _collider.Pose.Position + Vector3.UnitY * CAMERA_OFFSET;
        }
    }
}
