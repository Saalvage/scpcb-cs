namespace SCPCB.Entities; 

public interface IPriorizableEntity : IComparable<IPriorizableEntity>, IEntity {
    int Priority => 0;

    /// <summary>
    /// Static priority. Lower is handled first. This value is only considered during insertion.
    /// </summary>
    int IComparable<IPriorizableEntity>.CompareTo(IPriorizableEntity? other) => Priority.CompareTo(other!.Priority);
}
