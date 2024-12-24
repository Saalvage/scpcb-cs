using System.Numerics;
using BepuPhysics;
using BepuUtilities;

namespace SCPCB.Physics; 

public struct PoseIntegratorCallbacks : IPoseIntegratorCallbacks {
    public Vector3 Gravity { get; set; } = new(0, -9.81f, 0);

    private float _linearDampingVal = 1 - .03f;
    public float LinearDamping {
        readonly get => _linearDampingVal;
        set {
            if (value is >= 0 and <= 1) {
                _linearDampingVal = 1 - value;
            } else {
                throw new ArgumentException("Must be within in the range [0, 1]", nameof(value));
            }
        }
    }

    private float _angularDampingVal = 1 - .03f;
    public float AngularDamping {
        readonly get => _angularDampingVal;
        set {
            if (value is >= 0 and <= 1) {
                _angularDampingVal = 1 - value;
            } else {
                throw new ArgumentException("Must be within in the range [0, 1]", nameof(value));
            }
        }
    }

    private Vector3Wide _gravityWideDt;
    private Vector<float> _linearDampingDt;
    private Vector<float> _angularDampingDt;

    public PoseIntegratorCallbacks() { }

    public readonly void Initialize(Simulation simulation) { }
    
    public void PrepareForIntegration(float dt) {
        _linearDampingDt = new(MathF.Pow(_linearDampingVal, dt));
        _angularDampingDt = new(MathF.Pow(_angularDampingVal, dt));
        _gravityWideDt = Vector3Wide.Broadcast(Gravity * dt);
    }
    
    public readonly void IntegrateVelocity(Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation,
        BodyInertiaWide localInertia, Vector<int> integrationMask, int workerIndex, Vector<float> dt, ref BodyVelocityWide velocity) {
        velocity.Linear = (velocity.Linear + _gravityWideDt) * _linearDampingDt;
        velocity.Angular *= _angularDampingDt;
    }

    public readonly AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;
    public readonly bool AllowSubstepsForUnconstrainedBodies => false;
    public readonly bool IntegrateVelocityForKinematics => false;
}
