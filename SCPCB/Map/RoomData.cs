using System.Numerics;
using BepuPhysics.Collidables;
using SCPCB.Entities;
using SCPCB.Graphics;
using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.Shaders.Utility;
using SCPCB.Map.Entities;
using SCPCB.Physics;
using SCPCB.Physics.Primitives;
using SCPCB.Utility;

namespace SCPCB.Map;

public interface IRoomData : IDisposable {
    public IRoomInstance Instantiate(Vector3 offset, Quaternion rotation);
}

public class RoomData : Disposable, IRoomData {
    private readonly MeshInfo[] _meshes;
    private readonly ICBShape<Mesh>? _visibleCollIndex;
    private readonly ICBShape<Mesh>? _invisibleCollIndex;
    private readonly IMapEntityData[] _mapEntities;

    private readonly GraphicsResources _gfxRes;
    private readonly PhysicsResources _physics;

    public RoomData(GraphicsResources gfxRes, PhysicsResources physics, MeshInfo[] meshes, Mesh? visibleCollision, Mesh? invisibleCollision,
            IMapEntityData[] mapEntities) {
        _gfxRes = gfxRes;
        _physics = physics;
        _meshes = meshes;
        _visibleCollIndex = visibleCollision.HasValue ? new CBShape<Mesh>(physics, visibleCollision.Value) : null;
        _invisibleCollIndex = invisibleCollision.HasValue ? new CBShape<Mesh>(physics, invisibleCollision.Value) : null;
        _mapEntities = mapEntities;
    }

    public readonly record struct MeshInfo(ICBMesh Geometry, ICBMaterial Material, Vector3 PositionInRoom, bool IsOpaque);

    public IRoomInstance Instantiate(Vector3 offset, Quaternion rotation)
        => new RoomInstance(_physics, this, _meshes, _visibleCollIndex, _invisibleCollIndex, offset, rotation, _mapEntities
            .Select(x => x.Instantiate(_gfxRes, _physics, new(offset, rotation))).ToArray());

    protected override void DisposeImpl() {
        _visibleCollIndex?.Dispose();
        _invisibleCollIndex?.Dispose();
        foreach (var mesh in _meshes) {
            mesh.Geometry.Dispose();
            mesh.Material.Dispose();
        }
    }
}

public interface IRoomInstance : IConstantProvider<IWorldMatrixConstantMember, Matrix4x4>, I3DModelHolder, IEntityHolder { }

public class RoomInstance : Disposable, IRoomInstance {
    private record Model3D(Vector3 Position, ICBModel Model, bool IsOpaque) : I3DModel;

    IEnumerable<I3DModel> I3DModelHolder.Models => Models;
    public I3DModel[] Models { get; }

    IEnumerable<IEntity> IEntityHolder.Entities => Entites;
    public IMapEntity[] Entites { get; }

    private readonly Matrix4x4 _transform;
    private readonly RoomData _data; // Keep alive.

    private readonly CBStatic? _visibleColl;
    private readonly CBStatic? _invisibleColl;

    public RoomInstance(PhysicsResources physics, RoomData data, RoomData.MeshInfo[] meshes, ICBShape<Mesh>? visibleCollShape, ICBShape<Mesh>? invisibleCollShape,
            Vector3 offset, Quaternion rotation, IMapEntity[] mapEntities) {
        _data = data;

        // TODO: Support different shaders here.
        var constants = meshes[0].Material.Shader.TryCreateInstanceConstants();

        Models = meshes.Select(x => (I3DModel)new Model3D(Vector3.Transform(x.PositionInRoom, rotation) + offset,
                x.Geometry.CreateModel(x.Material, constants), x.IsOpaque))
            .ToArray();

        Entites = mapEntities;

        // TODO: This illustrates the shittyness of the current design.
        foreach (var m in Models) {
            m.Model.ConstantProviders.Add(this);
        }

        _visibleColl = visibleCollShape?.CreateStatic(new(offset, rotation));
        _invisibleColl = invisibleCollShape?.CreateStatic(new(offset, rotation));
        if (_invisibleColl != null) {
            _invisibleColl.IsInvisible = true;
        }

        _transform = Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(offset);
    }
    
    public Matrix4x4 GetValue(float interp) => _transform;

    protected override void DisposeImpl() {
        _visibleColl?.Dispose();
        _invisibleColl?.Dispose();
    }
}
