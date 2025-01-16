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
    private readonly CBShape<Mesh>? _visibleCollision;
    private readonly CBShape<Mesh>? _invisibleCollision;
    private readonly IMapEntityData[] _mapEntities;

    private readonly GraphicsResources _gfxRes;
    private readonly PhysicsResources _physics;

    public RoomData(GraphicsResources gfxRes, PhysicsResources physics, MeshInfo[] meshes, Mesh? visibleCollision, Mesh? invisibleCollision,
            IMapEntityData[] mapEntities) {
        _gfxRes = gfxRes;
        _physics = physics;
        _meshes = meshes;
        _visibleCollision = visibleCollision.HasValue ? new CBShape<Mesh>(physics, visibleCollision.Value) : null;
        _invisibleCollision = invisibleCollision.HasValue ? new CBShape<Mesh>(physics, invisibleCollision.Value) : null;
        _mapEntities = mapEntities;
    }

    public readonly record struct MeshInfo(IMeshMaterial Mesh, Vector3 PositionInRoom, bool IsOpaque) {
        public static MeshInfo Create<TVertex>(ICBMesh<TVertex> mesh, ICBMaterial<TVertex> mat, Vector3 pos, bool isOpaque)
            where TVertex : unmanaged
            => new(new MeshMaterial<TVertex>(mesh, mat), pos, isOpaque);
    }

    public IRoomInstance Instantiate(Vector3 offset, Quaternion rotation)
        => new RoomInstance(this, _meshes, _visibleCollision, _invisibleCollision, offset, rotation, _mapEntities
            .Select(x => x.Instantiate(_gfxRes, _physics, new(offset, rotation))).ToArray());

    protected override void DisposeImpl() {
        _visibleCollision?.Dispose();
        _invisibleCollision?.Dispose();
        foreach (var mesh in _meshes) {
            mesh.Mesh.Mesh.Dispose();
            // Materials are cached.
        }
    }
}

public interface IRoomInstance : IConstantProvider<IWorldMatrixConstantMember, Matrix4x4>, ISortableMeshInstanceHolder, IEntityHolder;

public class RoomInstance : Disposable, IRoomInstance {
    IEnumerable<ISortableMeshInstance> ISortableMeshInstanceHolder.Instances => Instances;
    public IReadOnlyList<ISortableMeshInstance> Instances { get; }

    IEnumerable<IEntity> IEntityHolder.Entities => Entities;
    public IReadOnlyList<IMapEntity> Entities { get; }

    private readonly Matrix4x4 _transform;
    private readonly RoomData _data; // Keep alive.

    private readonly CBStatic? _visibleColl;
    // TODO: We only expose this to allow for visualizing it for debugging.
    public CBStatic? InvisibleCollision { get; }

    public Transform Transform { get; }

    public RoomInstance(RoomData data, RoomData.MeshInfo[] meshes, ICBShape<Mesh>? visibleCollShape, ICBShape<Mesh>? invisibleCollShape,
        Vector3 offset, Quaternion rotation, IMapEntity[] mapEntities) {
        _data = data;

        Transform = new(offset, rotation);

        Instances = meshes
            .Zip(meshes.Select(x => x.Mesh).Instantiate())
            .Select(ISortableMeshInstance (x) => new SortableMeshInstance(x.Second,
                Vector3.Transform(x.First.PositionInRoom, rotation) + offset, x.First.IsOpaque))
            .ToArray();

        Entities = mapEntities;

        // TODO: This illustrates the shittyness of the current design.
        foreach (var m in Instances) {
            m.MeshInstance.ConstantProviders.Add(this);
        }

        _visibleColl = visibleCollShape?.CreateStatic(new(offset, rotation));
        InvisibleCollision = invisibleCollShape?.CreateStatic(new(offset, rotation));
        InvisibleCollision?.SetProperty<Visibility, bool>(true);

        _transform = Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(offset);
    }

    public Matrix4x4 GetValue(float interp) => _transform;

    protected override void DisposeImpl() {
        _visibleColl?.Dispose();
        InvisibleCollision?.Dispose();
    }
}
