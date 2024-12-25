using SCPCB.Graphics.Primitives;
using SCPCB.Utility;

namespace SCPCB.Graphics.ModelCollections;

public class InterpolatedModelCollection : ModelCollection {
    private Transform _previousWorldTransform;
    private Transform _currentWorldTransform;

    public InterpolatedModelCollection(IReadOnlyList<ICBModel> models) : base(models) { }

    /// <summary>
    /// Intended for non-smooth transformations as to not affect interpolation.
    /// </summary>
    /// <param name="transform"></param>
    public void Teleport(Transform transform) {
        WorldTransform = transform;
        _previousWorldTransform = transform;
        _currentWorldTransform = transform;
    }

    /// <summary>
    /// To be called after the transform has been updated.
    /// </summary>
    protected void UpdateTransform() {
        _previousWorldTransform = _currentWorldTransform;
        _currentWorldTransform = WorldTransform;
    }

    protected override Transform GetUsedTransform(double interpolation) {
        return Transform.Lerp(_previousWorldTransform, _currentWorldTransform, (float)interpolation);
    }
}
