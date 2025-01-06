using System.Numerics;
using SCPCB.Entities;
using SCPCB.Graphics;
using SCPCB.Physics;
using SCPCB.Scenes;

namespace SCPCB.PlayerController;

public partial class Player : IUpdatable, ITickable {
    private readonly IScene _scene;
    private readonly PhysicsResources _physics;

    public float Yaw { get; private set; }
    public float Pitch { get; private set; }

    public PerspectiveCamera Camera { get; } = new();

    public float BlinkTimer { get; private set; } = 20f;

    public Player(IScene scene) {
        _scene = scene;
        _physics = scene.GetEntitiesOfType<PhysicsResources>().Single();
        _collider = CreateCollider(_physics);
    }  

    public void HandleMouse(Vector2 delta) {
        Yaw = (Yaw - delta.X) % (2 * MathF.PI);
        Pitch = (Pitch + delta.Y) % (2 * MathF.PI);
        Camera.Rotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f);
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
