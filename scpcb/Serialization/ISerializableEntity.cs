using scpcb.Entities;

namespace scpcb.Serialization;

public interface ISerializableEntity : IEntity {
    SerializableData Serialize();
}

public interface ISerializableEntity<TSelf, TData> : ISerializableEntity where TSelf : ISerializableEntity<TSelf, TData> where TData : BaseSerializableData<TData, TSelf> {
    SerializableData ISerializableEntity.Serialize() => Serialize();
    TData Serialize() {
        var ret = SerializeImpl();
        ret.HashCode = GetHashCode();
        return ret;
    }

    protected TData SerializeImpl();
}
