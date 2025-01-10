using System.Reflection;
using BepuPhysics;
using BepuPhysics.Collidables;
using SCPCB.Utility;

namespace SCPCB.Physics.Primitives;

public abstract class CBCollidable : Disposable, IEquatable<CollidableReference> {
    public PhysicsResources Physics { get; }

    public ICBShape Shape { get; }

    private static readonly MethodInfo _setProperty = typeof(PhysicsResources).GetMethod(nameof(PhysicsResources.SetProperty))!;

    private readonly Dictionary<Type, object> _properties = [];

    public abstract RigidPose Pose { get; set; }

    public bool IsAttached { get; protected set; }

    protected CBCollidable(PhysicsResources physics, ICBShape shape) {
        Physics = physics;
        Shape = shape;
    }

    protected override void DisposeImpl() {
        Detach();
    }

    public void Attach() {
        if (IsAttached) {
            return;
        }

        AttachImpl();

        // We need to set it again because we might have received a different reference.
        ReapplyProperties();
        IsAttached = true;
    }

    public void Detach() {
        if (!IsAttached) {
            return;
        }

        DetachImpl();

        // TODO: Can handles be reassigned to a different entity? This could be cause for some nasty bugs if it were the case.
        // We know that detaching and reattaching requires resetting of properties to make sure they're correct.
        // If they CANNOT, then this is unnecessary.
        ResetProperties();
        IsAttached = false;
    }

    protected abstract void AttachImpl();
    protected abstract void DetachImpl();

    public void SetProperty<T, TVal>(TVal t) where TVal : unmanaged {
        Physics.SetProperty<T, TVal>(GetCollidableReference(), t);
        _properties[typeof(T)] = t;
    }

    public TVal GetProperty<T, TVal>() where T : struct => (TVal)_properties.GetValueOrDefault(typeof(T))!;

    protected void ReapplyProperties() {
        foreach (var (t, v) in _properties) {
            _setProperty
                .MakeGenericMethod(t, v.GetType())
                .Invoke(Physics, [GetCollidableReference(), v]);
        }
    }

    protected void ResetProperties() {
        foreach (var (t, v) in _properties) {
            var tVal = v.GetType();
            _setProperty
                .MakeGenericMethod(t, tVal)
                .Invoke(Physics, [GetCollidableReference(), Activator.CreateInstance(tVal)]);
        }
    }

    protected abstract CollidableReference GetCollidableReference();

    public static bool operator ==(CBCollidable a, CollidableReference b) => a.Equals(b);
    public static bool operator !=(CBCollidable a, CollidableReference b) => !(a == b);
    public static bool operator ==(CollidableReference a, CBCollidable b) => b == a;
    public static bool operator !=(CollidableReference a, CBCollidable b) => b != a;

    public abstract bool Equals(CollidableReference other);
}
