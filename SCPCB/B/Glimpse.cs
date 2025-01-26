using System.Numerics;
using SCPCB.Entities;
using SCPCB.Graphics;
using SCPCB.Graphics.Primitives;
using SCPCB.Physics;
using SCPCB.PlayerController;
using SCPCB.Scenes;
using SCPCB.Utility;
using ShaderGen;

namespace SCPCB.B;

class Glimpse : IEntityHolder, ITickable {
    private readonly IScene _scene;
    private readonly Player _player;
    private readonly PhysicsResources _physics;

    private readonly Billboard _billboard;
    public IEnumerable<IEntity> Entities { get; }

    public Transform WorldTransform {
        get => _billboard.WorldTransform;
        set => _billboard.WorldTransform = value;
    }

    public Glimpse(Scene3D scene, ICBTexture texture) {
        _scene = scene;
        _player = scene.GetEntitiesOfType<Player>().Single();
        _physics = scene.Physics;
        Entities = [_billboard = Billboard.Create(scene.Graphics, texture, true)];
    }

    public void Tick() {
        if (BHelpers.GetFloor(_player.Camera.Position) == BHelpers.GetFloor(_billboard.WorldTransform.Position)
            && Vector2.DistanceSquared(_player.Position.XZ(), _billboard.WorldTransform.Position.XZ()) < 2.3f
            && !_physics.RayCastVisible(_player.Camera.Position, _billboard.WorldTransform.Position).HasValue) {
            _scene.RemoveEntity(this);
            Console.WriteLine("GOODBYE!");
        }
    }
}
