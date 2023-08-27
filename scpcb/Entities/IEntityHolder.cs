namespace scpcb.Entities; 

/// <summary>
/// Implementers do not need to dispose their held entities.
/// The scene takes care of that.
/// </summary>
public interface IEntityHolder : IEntity {
    IEnumerable<IEntity> Entities { get; }
}
