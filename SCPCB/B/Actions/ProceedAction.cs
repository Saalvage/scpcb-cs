using SCPCB.Scenes;

namespace SCPCB.B.Actions;

[FixedFloorActionInfo(0)]
public class ProceedAction : RadioActionBase {
    public override string PredeterminedFloor => "map0";
    public ProceedAction(IScene scene) : base(scene, "Assets/087-B/Sounds/radio1.ogg", 150) { }
}
