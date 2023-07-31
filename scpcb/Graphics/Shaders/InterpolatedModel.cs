namespace scpcb.Graphics.Shaders; 

public class InterpolatedModel : Model {
    private Transform _previousWorldTransform;
    private Transform _currentWorldTransform;

    public InterpolatedModel(params ICBMesh[] meshes) : base(meshes) {
        // TODO: The values used for interpolation start out wrongly.
    }

    /// <summary>
    /// Intended for non-smooth transformations as to not affect interpolation.
    /// </summary>
    /// <param name="trans"></param>
    public void Teleport(Transform trans) {
        WorldTransform = trans;
        _previousWorldTransform = trans;
        _currentWorldTransform = trans;
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
