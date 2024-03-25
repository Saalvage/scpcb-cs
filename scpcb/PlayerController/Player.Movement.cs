using System.Diagnostics;
using System.Numerics;
using scpcb.Utility;

namespace scpcb.PlayerController;

public partial class Player {
    public bool Noclip { get; set; } = true;
    public bool IsSprinting { get; set; } = false;
    public bool HasInfiniteStamina { get; set; } = true;
    public float BaseSpeed { get; } = 10;

    public Vector2 MoveDir { get; set; } = Vector2.Zero;

    public float Stamina { get; private set; } = 20f;
    public float MaxStamina { get; } = 100f;

    private void UpdateMovement(float delta) {
        var fpsFactor = delta * Helpers.DELTA_TO_FPS_FACTOR_FACTOR;

        Debug.Assert(MoveDir.LengthSquared() <= 1f);

        if (HasInfiniteStamina) {
            Stamina += MathF.Min(MaxStamina, (MaxStamina - Stamina) * 0.01f * fpsFactor);
        }

        if (MoveDir != Vector2.Zero) {
            var d = Vector3.Transform(new(MoveDir.X, 0, MoveDir.Y), Camera.Rotation);
            if (!Noclip) {
                d.Y = 0;
                d = Vector3.Normalize(d) * MoveDir.Length();
            }
            Camera.Position += d * delta * (IsSprinting && Stamina > 0 ? 2.5f : 1f) * BaseSpeed;

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
    }
}
