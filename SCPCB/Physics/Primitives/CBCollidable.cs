using System.Reflection;
using BepuPhysics.Collidables;
using SCPCB.Utility;

namespace SCPCB.Physics.Primitives;

public abstract class CBCollidable : Disposable {
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
}
