using scpcb.Graphics;
using scpcb.Scenes;
using System.Text.Json.Serialization;

namespace scpcb.Serialization;

[JsonPolymorphic]
public abstract record SerializableData {
    public long HashCode { get; set; }

    public ISerializableEntity Deserialize(GraphicsResources gfxRes, IScene scene, IReferenceResolverImpl refResolver) {
        var ret = DeserializeImpl(gfxRes, scene, refResolver);
        refResolver.SubmitEntity(HashCode, ret);
        return ret;
    }

    protected abstract ISerializableEntity DeserializeImpl(GraphicsResources gfxRes, IScene scene, IReferenceResolver refResolver);
}
