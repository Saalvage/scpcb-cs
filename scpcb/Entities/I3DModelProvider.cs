namespace scpcb.Entities; 

public interface I3DModelProvider : IEntity {
    // TODO: Consider a changing mode provider?
    // (Probably via events)
    public IEnumerable<I3DModel> Models { get; }
}
