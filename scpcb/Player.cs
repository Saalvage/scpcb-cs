using System.Diagnostics;
using System.Numerics;
using scpcb.Entities;
using scpcb.Graphics;
using scpcb.Graphics.UserInterface;
using scpcb.Scenes;

namespace scpcb;

public interface IPlayer : IEntity {
    public ICamera Camera { get; }

    void HandleMouse(Vector2 delta);
    void HandleMove(Vector2 dir, float delta);
    void TryPick();
}

public class Player : IPlayer, IUpdatable {
    public ICamera Camera { get; } = new PerspectiveCamera {
        Position = new(0, 0, -5),
    };

    private readonly IScene _scene;
    private IUIElement _currHand;

    private float _yaw;
    private float _pitch;

    private IPickableEntity? _closestPickable;

    public bool Noclip { get; set; } = true;
    public float Speed { get; set; } = 25f;

    public Player(IScene scene) {
        _scene = scene;
    }

    public void HandleMouse(Vector2 delta) {
        _yaw -= delta.X;
        _pitch += delta.Y;
        Camera.Rotation = Quaternion.CreateFromYawPitchRoll(_yaw, _pitch, 0f);
    }

    public void HandleMove(Vector2 dir, float delta) {
        Debug.Assert(dir.LengthSquared() <= 1f);
        var d = Vector3.Transform(new(dir.X, 0, dir.Y), Camera.Rotation);
        if (!Noclip) {
            d.Y = 0;
            d = Vector3.Normalize(d) * dir.Length();
        }
        Camera.Position += d * delta * Speed;
    }

    public void Update(float delta) {
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

    public void TryPick() {
        _closestPickable?.OnPicked(this);
    }
}
