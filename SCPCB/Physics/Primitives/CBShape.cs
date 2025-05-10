using System.Numerics;
using BepuPhysics.Collidables;
using BepuUtilities;
using SCPCB.Utility;

namespace SCPCB.Physics.Primitives; 

public interface ICBShape : IDisposable {
    PhysicsResources Physics { get; }
    IShape Shape { get; }
    TypedIndex ShapeIndex { get; }
    bool IsConvex { get; }
    ICBShape GetScaledClone(Vector3 scale);
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

    public bool IsConvex => _shape is IConvexShape;

    private readonly Vector3 _scale;
    // Shared among one "family" of shapes.
    private readonly WeakDictionary<Vector3, ICBShape> _scaled;

    private CBShape(PhysicsResources physics, T shape, WeakDictionary<Vector3, ICBShape> scaled, Vector3 scale) {
        Physics = physics;
        _shape = shape;
        ShapeIndex = physics.Simulation.Shapes.Add(shape);
        _scaled = scaled;
        _scale = scale;
    }

    public CBShape(PhysicsResources physics, T shape) : this(physics, shape, [], Vector3.One) {
        _scaled.Add(Vector3.One, this);
    }

    public ICBShape GetScaledClone(Vector3 scale) {
        if (scale == Vector3.Zero) {
            throw new InvalidOperationException("Cannot scale a shape with 0!");
        }

        var actualScale = scale * _scale;
        if (_scaled.TryGetValue(actualScale, out var shape)) {
            return shape;
        } else {
            // We're restricting ourselves to scaling, everything else seems too esoteric, I don't think we reasonably want to
            // shear our shapes. (I can't wait to be proven wrong.)
            ICBShape copy;
            switch (this) {
                case CBShape<ConvexHull> ch:
                    Matrix3x3.CreateScale(scale, out var mat);
                    ConvexHullHelper.CreateTransformedCopy(ch.Shape, in mat, Physics.Simulation.BufferPool, out var scaledHull);
                    copy = new CBShape<ConvexHull>(Physics, scaledHull, _scaled, actualScale);
                    break;
                case CBShape<Mesh> cm:
                    // TODO: This is probably not the best way to scale.
                    copy = new CBShape<Mesh>(Physics, Mesh.CreateWithSweepBuild(cm.Shape.Triangles, scale, Physics.BufferPool));
                    break;
                case CBShape<Box> cb:
                    copy = new CBShape<Box>(Physics, new(cb.Shape.Width * scale.X, cb.Shape.Height * scale.Y, cb.Shape.Length * scale.Z));
                    break;
                default:
                    throw new NotSupportedException($"Scaling a {GetType()} is not currently supported!");
            }

            _scaled.Add(actualScale, copy);
            return copy;
        }
    }

    protected override void DisposeImpl() {
        if (!Physics.IsDisposed) {
            Physics.Simulation.Shapes.RecursivelyRemoveAndDispose(ShapeIndex, Physics.Simulation.BufferPool);
        }
    }
}
