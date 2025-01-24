using System.Diagnostics;
using BepuPhysics;
using BepuPhysics.Collidables;
using System.Numerics;
using System.Runtime.InteropServices;
using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders.Vertices;
using SCPCB.Physics.Primitives;
using Veldrid;
using Helpers = SCPCB.Utility.Helpers;

namespace SCPCB.Physics;

public static class CBShapeExtensions {
    // These methods may appear redundant, however, I believe they provide a natural and intuitive way
    // to proceed when receiving/creating an ICBShape, without requiring concrete knowledge about
    // CBBody and CBStatic and prevent the question "what now?" from arising.
    public static CBConvexBody<T> CreateDynamic<T>(this ICBShape<T> shape, float mass)
        where T : unmanaged, IConvexShape
        => new(shape, mass);

    public static CBConvexBody<T> CreateKinematic<T>(this ICBShape<T> shape)
        where T : unmanaged, IConvexShape
        => new(shape) { IsKinematic = true };

    public static CBStatic CreateStatic(this ICBShape shape)
        => new(shape);

    public static CBStatic CreateStatic(this ICBShape shape, RigidPose pose)
        => new(shape, pose);

    public static BodyActivityDescription GetDefaultActivity<T>(this ICBShape<T> shape) where T : unmanaged, IConvexShape
        => BodyDescription.GetDefaultActivity(shape.Shape);

    public static BodyInertia ComputeInertia<T>(this ICBShape<T> shape, float mass) where T : unmanaged, IConvexShape
        => shape.Shape.ComputeInertia(mass);

    public static void ComputeBounds(this ICBShape shape, Quaternion rotation, out Vector3 min, out Vector3 max) {
        // TODO: Does this box the shape? Ideally it shouldn't.
        switch (shape.Shape) {
            case IConvexShape cs:
                cs.ComputeBounds(rotation, out min, out max);
                break;
            case Mesh mesh:
                mesh.ComputeBounds(rotation, out min, out max);
                break;
            default:
                throw new NotSupportedException($"Computing bounds for {shape.GetType()} is not currently supported!");
        }
    }

    // We slightly scale the vertices to prevent z-fighting.
    private const float DEBUG_MESH_SCALE_FACTOR = 1.001f;

    public static ICBMesh<VPositionNormal> CreateDebugMesh(this ICBShape<ConvexHull> shape, GraphicsDevice gfx) {
        ref var hull = ref shape.Shape;

        var verts = new VPositionNormal[hull.Points.Length * Vector<float>.Count];
        var indices = new List<uint>(hull.FaceToVertexIndicesStart.Length * 6);
        for (var i = 0; i < hull.FaceToVertexIndicesStart.Length; i++) {
            hull.GetVertexIndicesForFace(i, out var s);
            Debug.Assert(s.Length >= 3);
            hull.GetPoint(s[0], out var a);
            hull.GetPoint(s[1], out var b);
            hull.GetPoint(s[2], out var c);
            var normal = Helpers.ComputeNormal(a, b, c);
            for (uint j = 0; j < s.Length; j++) {
                verts[GetIndex(j)].Normal += normal;
            }

            for (uint j = 1; j < s.Length; j *= 2) {
                for (uint k = 0; k < s.Length - j; k += j * 2) {
                    indices.Add(GetIndex(k + 0 * j));
                    indices.Add(GetIndex(k + 1 * j));
                    var secondIndex = k + 2 * j;
                    indices.Add(GetIndex(secondIndex >= s.Length ? 0 : secondIndex));
                }
            }

            uint GetIndex(uint i) => (uint)(s[i].BundleIndex * Vector<float>.Count + s[i].InnerIndex);
        }
        for (var i = 0; i < verts.Length; i++) {
            hull.GetPoint(i, out var vec);
            verts[i] = new(vec * DEBUG_MESH_SCALE_FACTOR, Vector3.Normalize(verts[i].Normal));
        }
        return new CBMesh<VPositionNormal>(gfx, verts, CollectionsMarshal.AsSpan(indices));
    }

    public static ICBMesh<VPositionNormal> CreateDebugMesh(this ICBShape<Mesh> shape, GraphicsDevice gfx) {
        ref var mesh = ref shape.Shape;
        var scale = mesh.Scale * DEBUG_MESH_SCALE_FACTOR;

        var verts = new VPositionNormal[mesh.Triangles.Length * 3];
        var indices = new uint[mesh.Triangles.Length * 3];
        for (uint i = 0; i < mesh.Triangles.Length; i++) {
            ref var tri = ref mesh.Triangles[i];
            var normal = Helpers.ComputeNormal(tri.C, tri.B, tri.A);
            verts[i * 3 + 0] = new(tri.C * scale, normal);
            verts[i * 3 + 1] = new(tri.B * scale, normal);
            verts[i * 3 + 2] = new(tri.A * scale, normal);
            indices[i * 3 + 0] = i * 3 + 0;
            indices[i * 3 + 1] = i * 3 + 1;
            indices[i * 3 + 2] = i * 3 + 2;
        }
        return new CBMesh<VPositionNormal>(gfx, verts, indices);
    }
}
