using System.Diagnostics;
using System.Numerics;
using scpcb.Graphics;

namespace scpcb;

public interface ICharacterController {
    public ICamera Camera { get; }

    void HandleMouse(Vector2 delta);
    void HandleMove(Vector2 dir, float delta);
}

public class CharacterController : ICharacterController {
    public ICamera Camera { get; }

    private float _yaw;
    private float _pitch;

    public bool Noclip { get; set; } = true;
    public float Speed { get; set; } = 250f;

    public CharacterController() {
        Camera = new PerspectiveCamera {
            Position = new(0, 0, -5),
        };
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
}
