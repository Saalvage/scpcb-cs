using System.Numerics;
using BepuPhysics.Collidables;
using scpcb.Entities;
using scpcb.Graphics;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Physics;
using scpcb.Utility;

namespace scpcb;

public class RoomData : Disposable {
    private readonly MeshInfo[] _meshes;
    private readonly TypedIndex _collIndex;

    private readonly PhysicsResources _physics;

    public RoomData(PhysicsResources physics, MeshInfo[] meshes, Mesh collision) {
        _physics = physics;
        _meshes = meshes;
        _collIndex = physics.Simulation.Shapes.Add(collision);
    }

    public readonly record struct MeshInfo(ICBMesh Geometry, ICBMaterial Material, Vector3 PositionInRoom, bool IsOpaque);

    public RoomInstance Instantiate(Vector3 offset, Quaternion rotation)
        => new(_physics, this, _meshes, _collIndex, offset, rotation);

    protected override void DisposeImpl() {
        _physics.Simulation.Shapes.RecursivelyRemoveAndDispose(_collIndex, _physics.BufferPool);
        foreach (var mesh in _meshes) {
            mesh.Geometry.Dispose();
            mesh.Material.Dispose();
        }
    }
}

public class RoomInstance : IConstantProvider<IWorldMatrixConstantMember, Matrix4x4> {
    private record Model3D(Vector3 Position, ICBModel Model) : I3DModel;

    public I3DModel[] Models { get; }

    private readonly Matrix4x4 _transform;
    private readonly RoomData _data; // Keep alive.

    public RoomInstance(PhysicsResources physics, RoomData data, RoomData.MeshInfo[] meshes, TypedIndex collIndex, Vector3 offset, Quaternion rotation) {
        _data = data;

        // TODO: Support different shaders here.
        var constants = meshes[0].Material.Shader.TryCreateInstanceConstants();

        Models = meshes.Select(x => (I3DModel)new Model3D(Vector3.Transform(x.PositionInRoom, rotation) + offset,
                x.Geometry.CreateModel(x.Material, constants, x.IsOpaque)))
            .ToArray();

        // TODO: This illustrates the current shittyness of the current design.
        foreach (var m in Models) {
            m.Model.ConstantProviders.Add(this);
        }

        physics.Simulation.Statics.Add(new(offset, rotation, collIndex));

        _transform = Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(offset);
    }
    
    public Matrix4x4 GetValue(float interp) => _transform;
}
