using System.Numerics;
using BepuPhysics.Collidables;
using scpcb.Entities;
using scpcb.Graphics;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Graphics.Shaders.Utility;
using scpcb.Map.Entities;
using scpcb.Physics;
using scpcb.Utility;

namespace scpcb.Map;

public interface IRoomData : IDisposable {
    public IRoomInstance Instantiate(Vector3 offset, Quaternion rotation);
}

public class RoomData : Disposable, IRoomData {
    private readonly MeshInfo[] _meshes;
    private readonly TypedIndex? _visibleCollIndex;
    private readonly TypedIndex? _invisibleCollIndex;
    private readonly IMapEntityData[] _mapEntities;

    private readonly GraphicsResources _gfxRes;
    private readonly PhysicsResources _physics;

    public RoomData(GraphicsResources gfxRes, PhysicsResources physics, MeshInfo[] meshes, Mesh? visibleCollision, Mesh? invisibleCollision,
            IMapEntityData[] mapEntities) {
        _gfxRes = gfxRes;
        _physics = physics;
        _meshes = meshes;
        _visibleCollIndex = visibleCollision.HasValue ? physics.Simulation.Shapes.Add(visibleCollision.Value) : null;
        _invisibleCollIndex = invisibleCollision.HasValue ? physics.Simulation.Shapes.Add(invisibleCollision.Value) : null;
        _mapEntities = mapEntities;
    }

    public readonly record struct MeshInfo(ICBMesh Geometry, ICBMaterial Material, Vector3 PositionInRoom, bool IsOpaque);

    public IRoomInstance Instantiate(Vector3 offset, Quaternion rotation)
        => new RoomInstance(_physics, this, _meshes, _visibleCollIndex, _invisibleCollIndex, offset, rotation, _mapEntities
            .Select(x => x.Instantiate(_gfxRes, _physics, new(offset, rotation))).ToArray());

    protected override void DisposeImpl() {
        if (_visibleCollIndex.HasValue) {
            _physics.Simulation.Shapes.RecursivelyRemoveAndDispose(_visibleCollIndex.Value, _physics.BufferPool);
        }
        if (_invisibleCollIndex.HasValue) {
            _physics.Simulation.Shapes.RecursivelyRemoveAndDispose(_invisibleCollIndex.Value, _physics.BufferPool);
        }
        foreach (var mesh in _meshes) {
            mesh.Geometry.Dispose();
            mesh.Material.Dispose();
        }
    }
}

public interface IRoomInstance : IConstantProvider<IWorldMatrixConstantMember, Matrix4x4>, I3DModelHolder, IEntityHolder { }

public class RoomInstance : IRoomInstance {
    private record Model3D(Vector3 Position, ICBModel Model) : I3DModel;

    IEnumerable<I3DModel> I3DModelHolder.Models => Models;
    public I3DModel[] Models { get; }

    IEnumerable<IEntity> IEntityHolder.Entities => Entites;
    public IMapEntity[] Entites { get; }

    private readonly Matrix4x4 _transform;
    private readonly RoomData _data; // Keep alive.

    public RoomInstance(PhysicsResources physics, RoomData data, RoomData.MeshInfo[] meshes, TypedIndex? visibleCollIndex, TypedIndex? invisibleCollIndex,
            Vector3 offset, Quaternion rotation, IMapEntity[] mapEntities) {
        _data = data;

        // TODO: Support different shaders here.
        var constants = meshes[0].Material.Shader.TryCreateInstanceConstants();

        Models = meshes.Select(x => (I3DModel)new Model3D(Vector3.Transform(x.PositionInRoom, rotation) + offset,
                x.Geometry.CreateModel(x.Material, constants, x.IsOpaque)))
            .ToArray();

        Entites = mapEntities;

        // TODO: This illustrates the current shittyness of the current design.
        foreach (var m in Models) {
            m.Model.ConstantProviders.Add(this);
        }

        if (visibleCollIndex.HasValue) {
            physics.Simulation.Statics.Add(new(offset, rotation, visibleCollIndex.Value));
        }
        if (invisibleCollIndex.HasValue) {
            physics.Visibility.Allocate(physics.Simulation.Statics.Add(new(offset, rotation, invisibleCollIndex.Value)))
                = new() { IsInvisible = true };
        }

        _transform = Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(offset);
    }
    
    public Matrix4x4 GetValue(float interp) => _transform;
}
