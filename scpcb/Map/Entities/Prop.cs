﻿using BepuPhysics;
using BepuPhysics.Collidables;
using scpcb.Entities;
using scpcb.Graphics;
using scpcb.Graphics.Caches;
using scpcb.Graphics.ModelCollections;
using scpcb.Physics;
using scpcb.Physics.Primitives;
using scpcb.Physics.Primitives;
using scpcb.Scenes;
using scpcb.Serialization;
using scpcb.Utility;

namespace scpcb.Map.Entities;

public class Prop : Disposable, IMapEntity, IEntityHolder, ISerializableEntity {
    public record PropData(string File, Transform Transform, BodyVelocity Velocity, bool IsStatic)
            : SerializableData {
        protected override ISerializableEntity DeserializeImpl(GraphicsResources gfxRes, IScene scene, IReferenceResolver refResolver) {
            var prop = new Prop(scene.GetEntitiesOfType<PhysicsResources>().Single(), File, Transform, IsStatic);
            if (prop.Models is PhysicsModelCollection pmc) {
                pmc.Body.Velocity = Velocity;
            }
            return prop;
        }
    }

    public const string PROP_PATH = "Assets/Props/";

    private readonly ICBShape<ConvexHull> _hull;

    private readonly string _file;
    
    // TODO: Keep alive, indicative of a design issue.
    private readonly ModelCache.CacheEntry _cacheEntry;

    public ModelCollection Models { get; }

    public Prop(PhysicsResources physics, string file, Transform transform, bool isStatic = false) {
        _file = file;

        _cacheEntry = physics.ModelCache.GetModel(PROP_PATH + file);
        var meshes = _cacheEntry.Models.Instantiate().ToArray();
        var hull = _cacheEntry.Collision;

        _hull = hull.CreateScaledCopy(transform.Scale);
        if (isStatic) {
            _hull.CreateStatic(new(transform.Position, transform.Rotation));
            Models = new(meshes) {
                WorldTransform = transform,
            };
        } else {
            var body = _hull.CreateDynamic(new(transform.Position, transform.Rotation), 1f);
            var pmc = new PhysicsModelCollection(physics, body, meshes);
            pmc.Teleport(transform);
            Models = pmc;
        }
    }

    public IEnumerable<IEntity> Entities {
        get {
            yield return Models;
        }
    }

    protected override void DisposeImpl() {
        _hull.Dispose();
    }

    public SerializableData SerializeImpl() {
        var pmc = Models as PhysicsModelCollection;
        return new PropData(_file, Models.WorldTransform, pmc?.Body.Velocity ?? default, pmc == null);
    }
}
