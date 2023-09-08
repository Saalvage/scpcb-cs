using scpcb.Entities;

namespace scpcb.Serialization;

public interface ISerializableEntity : IEntity {
    protected SerializableData SerializeImpl();

    SerializableData Serialize() {
        var ret = SerializeImpl();
        ret.HashCode = GetHashCode();
        return ret;
    }
}
