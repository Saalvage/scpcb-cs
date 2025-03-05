using SCPCB.Entities;

namespace SCPCB.B.Actions;

public interface IFloorAction : IEntity {
    string? PredeterminedFloor => "map";

    void OnEnter() { }
    void OnLeave() { }
}
