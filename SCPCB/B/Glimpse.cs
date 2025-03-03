using System.Numerics;
using SCPCB.Audio;
using SCPCB.Entities;
using SCPCB.Graphics;
using SCPCB.Graphics.Primitives;
using SCPCB.Physics;
using SCPCB.PlayerController;
using SCPCB.Scenes;
using SCPCB.Utility;
using ShaderGen;

namespace SCPCB.B;

class Glimpse : IEntityHolder, ITickable, ITransformable {
    private readonly IScene _scene;
    private readonly Player _player;
    private readonly PhysicsResources _physics;

    private readonly AudioFile _noSound;
    private readonly AudioChannel3D _channel;

    private readonly Billboard _billboard;
    public IEnumerable<IEntity> Entities { get; }

    public bool _disappared = false;

    public Transform WorldTransform {
        get => _billboard.WorldTransform;
        set => _billboard.WorldTransform = value;
    }

    public Glimpse(Scene3D scene, ICBTexture texture, AudioFile noSound) {
        _scene = scene;
        _player = scene.GetEntitiesOfType<Player>().Single();
        _physics = scene.Physics;
        _noSound = noSound;
        _channel = new();
        _channel.Parent = this;
        Entities = [_billboard = Billboard.Create(scene.Graphics, texture, true), _channel];
    }

    public void Tick() {
        if (!_disappared) {
            if (BHelpers.GetFloor(_player.Camera.WorldTransform.Position) == BHelpers.GetFloor(_billboard.WorldTransform.Position)
                && Vector2.DistanceSquared(_player.Position.XZ(), _billboard.WorldTransform.Position.XZ()) < 2.3f
                && !_physics.RayCastVisible(_player.Camera.WorldTransform.Position, _billboard.WorldTransform.Position).HasValue) {
                _scene.RemoveEntity(_billboard);
                _channel.Play(_noSound);
                _disappared = true;
            }
        } else if (!_channel.IsPlaying) {
            _scene.RemoveEntity(this);
        }

    }
}
