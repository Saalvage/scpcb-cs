using System.Numerics;
using SCPCB.Entities;
using SCPCB.Graphics;
using SCPCB.Physics;
using SCPCB.Scenes;

namespace SCPCB.PlayerController;

public partial class Player : IUpdatable, ITickable {
    private readonly IScene _scene;
    private readonly PhysicsResources _physics;

    private float _yaw;
    private float _pitch;

    public ICamera Camera { get; } = new PerspectiveCamera();

    public float BlinkTimer { get; private set; } = 20f;

    public Player(IScene scene) {
        _scene = scene;
        _physics = scene.GetEntitiesOfType<PhysicsResources>().Single();
        _collider = CreateCollider(_physics);
    }  

    public void HandleMouse(Vector2 delta) {
        _yaw -= delta.X;
        _pitch += delta.Y;
        Camera.Rotation = Quaternion.CreateFromYawPitchRoll(_yaw, _pitch, 0f);
    }

    public void Tick() {
        // TODO: Ehhhhh this kind of sucks, on one hand we're updating the camera's rotation every update
        // while we can only update the position every tick, since it's tied to the physics.
        // Either way this needs some more consideration, probably just want to interpolate the camera.
        UpdateMovement(Game.TICK_DELTA);
    }

    public void Update(float delta) {
        UpdatePickables();
    }
}
