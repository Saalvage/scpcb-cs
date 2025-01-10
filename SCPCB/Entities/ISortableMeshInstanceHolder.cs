namespace SCPCB.Entities; 

public interface ISortableMeshInstanceHolder : IEntity {
    // TODO: Consider a changing mode provider?
    // (Probably via events)
    public IEnumerable<ISortableMeshInstance> Models { get; }
}
