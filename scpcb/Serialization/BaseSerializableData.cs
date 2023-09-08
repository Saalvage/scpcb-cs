using scpcb.Graphics;
using scpcb.Scenes;
using System.Text.Json.Serialization;

namespace scpcb.Serialization;

[JsonPolymorphic]
public abstract record BaseSerializableData {
    public long HashCode { get; set; }

    public abstract ISerializableEntity Deserialize(GraphicsResources gfxRes, IScene scene, IReferenceResolver refResolver);
}

public abstract record BaseSerializableData<TSelf, TEntity> : BaseSerializableData where TSelf : BaseSerializableData<TSelf, TEntity> where TEntity : ISerializableEntity<TEntity, TSelf> {
    public override ISerializableEntity Deserialize(GraphicsResources gfxRes, IScene scene,
        IReferenceResolver refResolver) {
        return DeserializeImpl(gfxRes, scene, refResolver);
    }

    protected abstract TEntity DeserializeImpl(GraphicsResources gfxRes, IScene scene,
        IReferenceResolver refResolver);
}
