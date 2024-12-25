namespace SCPCB.Entities; 

public interface I3DModelHolder : IEntity {
    // TODO: Consider a changing mode provider?
    // (Probably via events)
    public IEnumerable<I3DModel> Models { get; }
}
