using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using scpcb.Entities;
using scpcb.Graphics;
using scpcb.Graphics.Assimp;
using scpcb.Graphics.ModelCollections;
using scpcb.Graphics.Shaders;
using scpcb.Physics;
using scpcb.Scenes;
using scpcb.Serialization;
using scpcb.Utility;

namespace scpcb.Map.Entities;

public class Prop : Disposable, IMapEntity, IEntityHolder, ISerializableEntity {
    public record PropData(string File, Transform Transform, BodyVelocity Velocity, bool IsStatic)
            : SerializableData {
        protected override ISerializableEntity DeserializeImpl(GraphicsResources gfxRes, IScene scene, IReferenceResolver refResolver) {
            var prop = new Prop(gfxRes, scene.GetEntitiesOfType<PhysicsResources>().Single(), File, Transform, IsStatic);
            if (prop.Models is PhysicsModelCollection pmc) {
                pmc.Body.Velocity = Velocity;
            }
            return prop;
        }
    }

    public const string PROP_PATH = "Assets/Props/";

    private readonly PhysicsResources _physics;
    private readonly TypedIndex _hullIndex;

    private readonly string _file;

    public ModelCollection Models { get; }

    public Prop(GraphicsResources gfxRes, PhysicsResources physics, string file, Transform transform, bool isStatic = false) {
        _physics = physics;
        _file = file;

        // TODO: Cache this shit!!
        var (meshes, hull) = new AutomaticAssimpMeshConverter<ModelShader, ModelShader.Vertex, (GraphicsResources, string)>((gfxRes, "Assets/Props/"))
            .LoadMeshes(gfxRes.GraphicsDevice, physics, PROP_PATH + file);

        Matrix3x3.CreateScale(transform.Scale, out var scaleMat);
        ConvexHullHelper.CreateTransformedCopy(hull, scaleMat, physics.BufferPool, out var scaledHull);
        hull.Dispose(_physics.BufferPool);

        _hullIndex = physics.Simulation.Shapes.Add(scaledHull);
        if (isStatic) {
            physics.Simulation.Statics.Add(new(transform.Position, transform.Rotation, _hullIndex));
            Models = new(meshes) {
                WorldTransform = transform,
            };
        } else {
            var body = physics.Simulation.Bodies.Add(new() {
                Pose = new(transform.Position, transform.Rotation),
                Velocity = new(Vector3.Zero),
                Activity = BodyDescription.GetDefaultActivity(scaledHull),
                Collidable = _hullIndex,
                LocalInertia = scaledHull.ComputeInertia(1f),
            });

            
            var pmc = new PhysicsModelCollection(physics, physics.Simulation.Bodies.GetBodyReference(body), meshes);
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
        _physics.Simulation.Shapes.RecursivelyRemoveAndDispose(_hullIndex, _physics.BufferPool);
    }

    public SerializableData SerializeImpl() {
        var pmc = Models as PhysicsModelCollection;
        return new PropData(_file, Models.WorldTransform, pmc?.Body.Velocity ?? default, pmc == null);
    }
}
