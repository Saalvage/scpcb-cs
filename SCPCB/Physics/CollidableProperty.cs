namespace SCPCB.Physics;

public interface ICollidableProperty<T> where T : unmanaged;
// TODO: Add support for default values and turn this into the more sensible "IsVisibleCollidableProperty"
public sealed record IsInvisibleProperty : ICollidableProperty<bool>;
public sealed record HasNoFrictionProperty : ICollidableProperty<bool>;
