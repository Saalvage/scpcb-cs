using scpcb.Entities;

namespace scpcb.Serialization;

public interface ISerializableEntity : IEntity {
    BaseSerializableData Serialize();
}

public interface ISerializableEntity<TSelf, TData> : ISerializableEntity where TSelf : ISerializableEntity<TSelf, TData> where TData : BaseSerializableData<TData, TSelf> {
    BaseSerializableData ISerializableEntity.Serialize() => Serialize();
    TData Serialize() {
        var ret = SerializeImpl();
        ret.HashCode = GetHashCode();
        return ret;
    }

    protected TData SerializeImpl();
}
