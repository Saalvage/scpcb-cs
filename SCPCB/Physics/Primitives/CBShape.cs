using BepuPhysics.Collidables;
using SCPCB.Utility;

namespace SCPCB.Physics.Primitives; 

public interface ICBShape : IDisposable {
    PhysicsResources Physics { get; }
    IShape Shape { get; }
    TypedIndex ShapeIndex { get; }
}

public interface ICBShape<T> : ICBShape where T : unmanaged, IShape {
    IShape ICBShape.Shape => Shape;
    new ref T Shape { get; }
}

public class CBShape<T> : Disposable, ICBShape<T> where T : unmanaged, IShape {
    public PhysicsResources Physics { get; }
    
    private T _shape;
    public ref T Shape => ref _shape;
    public TypedIndex ShapeIndex { get; }

    public CBShape(PhysicsResources physics, T shape) {
        Physics = physics;
        _shape = shape;
        ShapeIndex = physics.Simulation.Shapes.Add(shape);
    }

    protected override void DisposeImpl() {
        Physics.Simulation.Shapes.RecursivelyRemoveAndDispose(ShapeIndex, Physics.Simulation.BufferPool);
    }
}
