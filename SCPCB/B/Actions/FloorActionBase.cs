using SCPCB.Entities;

namespace SCPCB.B.Actions;

public class FloorActionBase : IEntity {
    public virtual string? PredeterminedFloor => "map";

    // Not in ctor to make downstream ctors slimmer and prevent potential modifications.
    public int Floor { protected get; init; }

    protected bool IsActive { get; private set; }

    public virtual void OnEnter() {
        IsActive = true;
    }

    public virtual void OnLeave() {
        IsActive = false;
    }
}
