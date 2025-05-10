using System.Diagnostics;
using System.Numerics;
using SCPCB.Physics;
using SCPCB.Physics.Primitives;
using Helpers = SCPCB.Utility.Helpers;
using SCPCB.Scenes;

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
                _collBody.Detach();
            } else {
                _collBody.Attach();
                _collBody.Pose = _collBody.Pose with { Position = Camera.WorldTransform.Position - Vector3.UnitY * _collInfo.CameraOffset };
                _collBody.Velocity = default;
                // Prevent being sucked onto the ground.
                _collider.IsFalling = true;
            }
            _noclip = value;
        }
    }

    public bool IsSprinting { get; set; } = false;
    public float BaseSpeed { get; } = 2;

    public Vector2 MoveDir { get; set; } = Vector2.Zero;

    private Vector3 _noclipVelocity;
    public Vector3 Velocity => Noclip ? _noclipVelocity : _collBody.Velocity.Linear;

    /// <summary>
    /// Center of the collider.
    /// </summary>
    public Vector3 Position => Noclip ? Camera.WorldTransform.Position - new Vector3(0, _collInfo.CameraOffset, 0) : _collBody.Pose.Position;

    public Vector3 FeetPosition => Position - new Vector3(0, _collInfo.HeightOffGround, 0);

    public bool HasInfiniteStamina { get; set; } = true;
    public float Stamina { get; private set; } = 20f;
    public float MaxStamina { get; } = 100f;

    private Collider _collider;
    // Convenience shorthands.
    private CBBody _collBody => _collider.Body;
    private Collider.Dimensions _collInfo => _collider.Info;

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
            var dir = Vector3.Transform(new(MoveDir.X, 0, MoveDir.Y), Camera.WorldTransform.Rotation);
            var vel = dir * speed * 5;
            _noclipVelocity = vel * Game.TICK_RATE;
            Camera.WorldTransform = Camera.WorldTransform with { Position = Camera.WorldTransform.Position + vel };
        } else {
            if (MoveDir == Vector2.Zero) {
                // TODO: Proper falling physics.
                if (!_collider.IsFalling) {
                    _collBody.Velocity = Vector3.Zero;
                }
            } else {
                var forward = Vector3.Transform(Vector3.UnitZ, Camera.WorldTransform.Rotation);
                forward.Y = 0;
                var right = Vector3.Cross(forward, Vector3.UnitY);
                var dir = Vector3.Normalize(forward) * MoveDir.Y + Vector3.Normalize(right) * -MoveDir.X;
                _collBody.Velocity = dir * speed * Game.TICK_RATE;
            }

            _collider.Tick();

            Camera.WorldTransform = Camera.WorldTransform with { Position = _collBody.Pose.Position + Vector3.UnitY * _collInfo.CameraOffset };
        }
    }
}
