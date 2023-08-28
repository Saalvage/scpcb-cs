using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using scpcb.Entities;
using scpcb.Graphics;
using scpcb.Graphics.Assimp;
using scpcb.Graphics.ModelCollections;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders;
using scpcb.Physics;
using scpcb.Utility;

namespace scpcb.Map.Entities;

public class Prop : IMapEntity, IEntityHolder {
    public const string PROP_PATH = "Assets/Props/";

    public ModelCollection Models { get; }

    public Prop(GraphicsResources gfxRes, PhysicsResources physics, string file, Transform transform, bool isStatic = false) {
        var mat = gfxRes.ShaderCache.GetShader<ModelShader, ModelShader.Vertex>().CreateMaterial(
            gfxRes.MissingTexture.AsEnumerableElement(), gfxRes.GraphicsDevice.Aniso4xSampler.AsEnumerableElement());
        var (meshes, hull) = new AutomaticAssimpMeshConverter<ModelShader, ModelShader.Vertex, ValueTuple<GraphicsResources, string>>((gfxRes, "Assets/Props/"))
            .LoadMeshes(gfxRes.GraphicsDevice, physics, PROP_PATH + file);

        // TODO: This leaks memory!
        Matrix3x3.CreateScale(transform.Scale, out var scaleMat);
        ConvexHullHelper.CreateTransformedShallowCopy(hull, scaleMat, physics.BufferPool, out var scaledHull);

        if (isStatic) {
            var typedIndex = physics.Simulation.Shapes.Add(scaledHull);
            physics.Simulation.Statics.Add(new(transform.Position, typedIndex));
            Models = new(meshes) {
                WorldTransform = transform,
            };
        } else {
            var body = physics.Simulation.Bodies.Add(BodyDescription.CreateConvexDynamic(new(transform.Position, transform.Rotation),
                new(Vector3.Zero), 1f, physics.Simulation.Shapes, scaledHull));

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
}
