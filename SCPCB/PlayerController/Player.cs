using System.Numerics;
using SCPCB.Audio;
using SCPCB.Entities;
using SCPCB.Graphics;
using SCPCB.Physics;
using SCPCB.Scenes;

namespace SCPCB.PlayerController;

public partial class Player : IUpdatable, ITickable, IEntityHolder {
    private readonly IScene _scene;
    private readonly PhysicsResources _physics;

    public float Yaw { get; private set; }
    public float Pitch { get; private set; }

    public PerspectiveCamera Camera { get; } = new();

    public float BlinkTimer { get; private set; } = 20f;

    private readonly AudioListener _listener = new();

    public IEnumerable<IEntity> Entities {
        get {
            yield return _listener;
        }
    }

    public Player(IScene scene, CollisionInfo info) {
        _scene = scene;
        _physics = scene.GetEntitiesOfType<PhysicsResources>().Single();
        _collider = CreateCollider(_physics, info);
        _listener.Parent = Camera;
    }  

    public void HandleMouse(Vector2 delta) {
        Yaw = (Yaw - delta.X) % (2 * MathF.PI);
        Pitch = (Pitch + delta.Y) % (2 * MathF.PI);
        Camera.WorldTransform = Camera.WorldTransform with { Rotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f) };
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
