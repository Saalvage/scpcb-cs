using System.Numerics;
using SCPCB.Entities;
using SCPCB.Graphics.Animation;
using SCPCB.Physics;
using SCPCB.Physics.Primitives;
using SCPCB.PlayerController;
using SCPCB.Scenes;
using SCPCB.Utility;

namespace SCPCB.B;

public class Enemy : ITickable, ITransformable, IEntityHolder {
    private readonly Player _player;

    private readonly AnimatedModel _model;
    private readonly Collider _collider;
    private readonly CBBodyTransformable _collWrapper;

    public Transform WorldTransform {
        get => _collWrapper.WorldTransform;
        set => _collWrapper.WorldTransform = value;
    }

    private float _killDistanceSquared = 0.8f * 0.8f;
    public float KillDistance {
        get => MathF.Sqrt(_killDistanceSquared);
        set => _killDistanceSquared = value * value;
    }

    public float Speed { get; set; } = 0.01f;

    public IEnumerable<IEntity> Entities { get; }

    public Enemy(IScene scene) {
        _player = scene.GetEntitiesOfType<Player>().Single();

        _collider = new(scene, new(1, 0.3f, 1.8f));
        // TODO: Have this functionality offered by the collider?
        // We generally duplicate the functionality of making something interpolatable quite a lot.
        _collWrapper = new(_collider.Body);

        var template = scene.Graphics.AnimatedModelCache.GetAnimatedModel("Assets/087-B/mental.b3d");
        _model = new(template) {
            Parent = _collWrapper,
            // The extra offset makes it sink into the ground, accurate to the original.
            LocalTransform = new(new(0, -_collider.Info.HeightOffGround - 0.1f, 0), Quaternion.Identity, new(0.17f)),
            Animation = template.Animations.Single().Value,
            Speed = 0,
        };
        Entities = [_model, _collider, _collWrapper];
    }

    public void Tick() {
        var q = Helpers.CreateLookAtQuaternion(
            (_player.FeetPosition - WorldTransform.Position) with { Y = 0 },
            Vector3.UnitY);

        _model.LocalTransform = _model.LocalTransform with { Rotation = q * Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI) };

        // The somewhat odd choices for points from which distances are measured are mostly copied as-is
        // from the original to allow for re-using the original distance values.
        if (!_collider.IsFalling) {
            if (Vector3.DistanceSquared(_player.Camera.WorldTransform.Position, _model.WorldTransform.Position) > 1.5 * 1.5) {
                _collider.Body.Velocity = Vector3.Transform(Vector3.UnitZ * Speed * Game.TICK_RATE, q);
            } else {
                _collider.Body.Velocity = Vector3.Zero;
            }
        }

        if (Vector3.DistanceSquared(_player.Position, WorldTransform.Position) < _killDistanceSquared) {
            Log.Error("DEAD");
        }
    }
}