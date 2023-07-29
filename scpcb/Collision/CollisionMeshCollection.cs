using System.Numerics;

namespace scpcb.Collision; 

public class CollisionMeshCollection {
    public static Vector3 Hit;

    private class CollisionMeshInstance {
        private readonly CollisionMesh _mesh;

        public Vector3[] Vertices { get; }
        public AABB AABB { private set; get; }

        public CollisionMeshInstance(CollisionMesh mesh) {
            _mesh = mesh;
            Vertices = new Vector3[mesh.Vertices.Length];
        }

        public void Update(Matrix4x4 mat) {
            for (var i = 0; i < Vertices.Length; i++) {
                Vertices[i] = Vector4.Transform(new Vector4(_mesh.Vertices[i], 1f), mat).Denormalize();
            }
            AABB = new(Vertices);
        }

        public CollideRRR.Collision Collide(AABB lineAABB, Vector3 begin, Vector3 end, float height, float radius) {
            AABB triBox;
            for (var i = 0; i < _mesh.Indices.Length; i += 3) {
                var v0 = Vertices[_mesh.Indices[i]];
                var v1 = Vertices[_mesh.Indices[i + 1]];
                var v2 = Vertices[_mesh.Indices[i + 2]];
                var testset = v0 == v1;
                triBox = new(v0);
                triBox.AddPoint(v1);
                triBox.AddPoint(v2);
                // TODO: More magic values!
                triBox.AddPoint(triBox.Min + new Vector3(-0.1f, -0.1f, -0.1f));
                triBox.AddPoint(triBox.Max + new Vector3(0.1f, 0.1f, 0.1f));
                if (triBox.Intersects(lineAABB)) {
                    var test = CollideRRR.TriangleCollide(begin, end, height, radius, v0, v1, v2);
                    if (test.Hit) {
                        Hit = test.End;
                        return test;
                    }
                }
            }

            return default;
        }
    }

    private CollisionMeshInstance[] _instances;
    private AABB _aabb;

    private bool needsReset = true;

    private Transform _transform = new();

    public CollisionMeshCollection(IEnumerable<CollisionMesh> meshes) {
        _instances = meshes
            .Select(x => new CollisionMeshInstance(x))
            .ToArray();
    }

    private void ResetIfRequired() {
        if (!needsReset) {
            return;
        }

        var mat = _transform.GetMatrix();
        foreach (var cmi in _instances) {
            cmi.Update(mat);
        }

        _aabb = new(_instances.Select(x => x.AABB));

        needsReset = false;
    }

    public CollideRRR.Collision Collide(Vector3 begin, Vector3 end, float height, float radius) {
        AABB lineAABB = new(begin);
        lineAABB.AddPoint(end);
        // TODO: What are these magic values?
        lineAABB.AddPoint(lineAABB.Min + new Vector3(-radius - 0.5f, -height * 0.5f - 0.5f, -radius - 0.5f));
        lineAABB.AddPoint(lineAABB.Max + new Vector3(radius + 0.5f, height * 0.5f + 0.5f, radius + 0.5f));

        ResetIfRequired();

        if (!lineAABB.Intersects(_aabb)) {
            return default;
        }

        return _instances.Select(x => x.Collide(lineAABB, begin, end, height, radius)).FirstOrDefault(x => x.Hit);
    }
}
