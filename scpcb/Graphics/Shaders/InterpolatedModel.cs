namespace scpcb.Graphics.Shaders; 

public class InterpolatedModel : Model {
    private Transform _previousWorldTransform;
    private Transform _currentWorldTransform;

    public InterpolatedModel(params ICBMesh[] meshes) : base(meshes) {
        // TODO: The values used for interpolation start out wrongly.
    }

    protected override Transform GetUsedTransform(double interpolation) {
        // Alternatively, this COULD be done in the Tick method, but that would require
        // the transform origin to tick before this, lest it lags behind by one frame.
        if (WorldTransform != _currentWorldTransform) {
            _previousWorldTransform = _currentWorldTransform;
            _currentWorldTransform = WorldTransform;
        }
        return Transform.Lerp(_previousWorldTransform, WorldTransform, (float)interpolation);
    }
}
