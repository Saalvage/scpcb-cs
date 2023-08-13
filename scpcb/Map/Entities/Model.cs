using System.Numerics;
using BepuPhysics;
using BepuUtilities;
using scpcb.Graphics;
using scpcb.Graphics.Assimp;
using scpcb.Graphics.ModelCollections;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders;
using scpcb.Physics;
using scpcb.Utility;

namespace scpcb.Map.Entities;

public class Model : IMapEntity {
    public const string PROP_PATH = "Assets/Props/";

    public ModelCollection Models { get; }

    public Model(GraphicsResources gfxRes, PhysicsResources physics, string file, Transform transform, bool isStatic = false) {
        var mat = gfxRes.ShaderCache.GetShader<ModelShaderGenerated>().CreateMaterial(
            gfxRes.MissingTexture.AsEnumerableElement());
        var (meshes, hull) = new AutomaticAssimpMeshConverter<ModelShader, ModelShader.Vertex, ICBMaterial<ModelShader.Vertex>>(mat)
            .LoadMeshes(gfxRes.GraphicsDevice, physics, PROP_PATH + file);

        // TODO: Do this properly.
        Span<float> scaleX = stackalloc float[Vector<float>.Count];
        Span<float> scaleY = stackalloc float[Vector<float>.Count];
        Span<float> scaleZ = stackalloc float[Vector<float>.Count];
        scaleX.Fill(transform.Scale.X);
        scaleY.Fill(transform.Scale.Y);
        scaleZ.Fill(transform.Scale.Z);
        Vector3Wide scalar = new() { X = new(scaleX), Y = new(scaleY), Z = new(scaleZ) }; ;
        for (var i = 0; i < hull.Points.Length; i++) { 
            hull.Points[i] *= scalar;
        }

        if (isStatic) {
            var typedIndex = physics.Simulation.Shapes.Add(hull);
            physics.Simulation.Statics.Add(new(transform.Position, typedIndex));
            Models = new(meshes) {
                WorldTransform = transform,
            };
        } else {
            var body = physics.Simulation.Bodies.Add(BodyDescription.CreateConvexDynamic(new(transform.Position, transform.Rotation),
                new(Vector3.Zero), 1f, physics.Simulation.Shapes, hull));

            Models = new PhysicsModelCollection(physics, physics.Simulation.Bodies.GetBodyReference(body), meshes) {
                WorldTransform = transform,
            };
        }
    }

    public static Model CreateEntity(GraphicsResources gfxRes, PhysicsResources physics, Transform roomTransform,
            IReadOnlyDictionary<string, object> data) {
        var pos = roomTransform.Position + (Vector3)data["position"];
        var rot = roomTransform.Rotation * (Quaternion)data["rotation"];
        var scale = roomTransform.Scale * (Vector3)data["scale"];
        return new(gfxRes, physics, (string)data["file"], new(pos, rot, scale));
    }
}
