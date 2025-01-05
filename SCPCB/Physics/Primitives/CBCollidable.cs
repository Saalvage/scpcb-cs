using System.Reflection;
using BepuPhysics.Collidables;
using SCPCB.Utility;

namespace SCPCB.Physics.Primitives;

public abstract class CBCollidable : Disposable, IEquatable<CollidableReference> {
    protected readonly PhysicsResources _physics;

    private static readonly MethodInfo _setProperty = typeof(PhysicsResources).GetMethod(nameof(PhysicsResources.SetProperty))!;

    private readonly Dictionary<Type, object> _properties = [];

    protected CBCollidable(PhysicsResources physics) {
        _physics = physics;
    }

    public void SetProperty<T, TVal>(TVal t) where TVal : unmanaged {
        _physics.SetProperty<T, TVal>(GetCollidableReference(), t);
        _properties[typeof(T)] = t;
    }

    public TVal GetProperty<T, TVal>() where T : struct => (TVal)_properties.GetValueOrDefault(typeof(T))!;

    protected void ReapplyProperties() {
        foreach (var (t, v) in _properties) {
            _setProperty
                .MakeGenericMethod(t, v.GetType())
                .Invoke(_physics, [GetCollidableReference(), v]);
        }
    }

    protected void ResetProperties() {
        foreach (var (t, v) in _properties) {
            var tVal = v.GetType();
            _setProperty
                .MakeGenericMethod(t, tVal)
                .Invoke(_physics, [GetCollidableReference(), Activator.CreateInstance(tVal)]);
        }
    }

    protected abstract CollidableReference GetCollidableReference();

    public static bool operator ==(CBCollidable a, CollidableReference b) => a.Equals(b);
    public static bool operator !=(CBCollidable a, CollidableReference b) => !(a == b);
    public static bool operator ==(CollidableReference a, CBCollidable b) => b == a;
    public static bool operator !=(CollidableReference a, CBCollidable b) => b != a;

    public abstract bool Equals(CollidableReference other);
}
