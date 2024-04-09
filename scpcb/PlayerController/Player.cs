using System.Numerics;
using scpcb.Entities;
using scpcb.Graphics;
using scpcb.Graphics.UserInterface;
using scpcb.Graphics.UserInterface.Primitives;
using scpcb.Scenes;

namespace scpcb.PlayerController;

public partial class Player : IUpdatable {
    private readonly IScene _scene;
    private IUIElement _currHand;

    private float _yaw;
    private float _pitch;

    public ICamera Camera { get; } = new PerspectiveCamera();

    public float BlinkTimer { get; private set; } = 20f;

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
}
