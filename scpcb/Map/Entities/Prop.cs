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
using scpcb.Utility;

namespace scpcb.Map.Entities;

public class Prop : Disposable, IMapEntity, IEntityHolder {
    public const string PROP_PATH = "Assets/Props/";

    private readonly PhysicsResources _physics;

    private TypedIndex _hullIndex;

    public ModelCollection Models { get; }

    public Prop(GraphicsResources gfxRes, PhysicsResources physics, string file, Transform transform, bool isStatic = false) {
        _physics = physics;

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

            Models = new PhysicsModelCollection(physics, physics.Simulation.Bodies.GetBodyReference(body), meshes) {
                WorldTransform = transform,
            };
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
}
