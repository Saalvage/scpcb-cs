using BepuPhysics;
using BepuPhysics.Collidables;
using scpcb.Utility;

namespace scpcb.Physics.Primitives; 

public interface ICBShape : IDisposable {
    Simulation Simulation { get; }
    IShape Shape { get; }
    TypedIndex ShapeIndex { get; }
}

public interface ICBShape<T> : ICBShape where T : unmanaged, IShape {
    IShape ICBShape.Shape => Shape;
    new ref T Shape { get; }
}

public class CBShape<T> : Disposable, ICBShape<T> where T : unmanaged, IShape {
    public Simulation Simulation { get; }
    
    private T _shape;
    public ref T Shape => ref _shape;
    public TypedIndex ShapeIndex { get; }

    public CBShape(Simulation sim, T shape) {
        Simulation = sim;
        _shape = shape;
        ShapeIndex = Simulation.Shapes.Add(shape);
    }

    protected override void DisposeImpl() {
        Simulation.Shapes.RecursivelyRemoveAndDispose(ShapeIndex, Simulation.BufferPool);
    }
}
