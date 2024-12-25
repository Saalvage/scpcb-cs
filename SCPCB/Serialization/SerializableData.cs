using System.Text.Json.Serialization;
using SCPCB.Graphics;
using SCPCB.Scenes;

namespace SCPCB.Serialization;

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
