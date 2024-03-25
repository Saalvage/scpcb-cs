using System.Diagnostics;
using System.Numerics;
using scpcb.Entities;
using scpcb.Graphics;
using scpcb.Graphics.UserInterface;
using scpcb.Scenes;
using scpcb.Utility;

namespace scpcb;

public class Player : IUpdatable {
    private readonly IScene _scene;
    private IUIElement _currHand;

    private float _yaw;
    private float _pitch;

    public ICamera Camera { get; } = new PerspectiveCamera();

    public float BlinkTimer { get; private set; } = 20f;

    #region Movement & Stamina

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
            Stamina += MathF.Min(100f, (100f - Stamina) * 0.01f * fpsFactor);
        }

        if (MoveDir != Vector2.Zero) {
            var d = Vector3.Transform(new(MoveDir.X, 0, MoveDir.Y), Camera.Rotation);
            if (!Noclip) {
                d.Y = 0;
                d = Vector3.Normalize(d) * MoveDir.Length();
            }
            Camera.Position += d * delta * (IsSprinting && Stamina > 0 ? 2.5f : 1f) * BaseSpeed;

            if (IsSprinting) {
                // TODO: Consider StaminaEffect
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

    #endregion

    #region Pickables

    private IPickableEntity? _closestPickable;

    private void UpdatePickables() {
        // Inefficient, but likely negligible impact on performance.
        var newClosestPickable = _scene.GetEntitiesOfType<IPickableEntity>()
            .Where(x => x.CanBePicked(this) && (x.Position - Camera.Position).LengthSquared() < 4)
            .MinBy(x => (Camera.Position - x.Position).LengthSquared());

        if (newClosestPickable != _closestPickable) {
            var ui = _scene.GetEntitiesOfType<UIManager>().Single();
            var children = ui.Root.Children;
            if (_closestPickable != null) {
                children.Remove(_currHand);
            }

            if (newClosestPickable != null) {
                _currHand = new TextureElement(ui.GraphicsResources, newClosestPickable.GetHandTexture()) { Alignment = Alignment.Center };
                ui.Root.Children.Add(_currHand);
            }
        }

        _closestPickable = newClosestPickable;
    }

    #endregion

    public Player(IScene scene) {
        _scene = scene;
    }

    public void HandleMouse(Vector2 delta) {
        _yaw -= delta.X;
        _pitch += delta.Y;
        Camera.Rotation = Quaternion.CreateFromYawPitchRoll(_yaw, _pitch, 0f);
    }

    public void Update(float delta) {
        UpdatePickables();
        UpdateMovement(delta);
    }

    public void TryPick() {
        _closestPickable?.OnPicked(this);
    }
}
