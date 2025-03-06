using SCPCB.Scenes;

namespace SCPCB.B.Actions;

[FixedFloorActionInfo(2, 3, 0.5f)]
public class Radio2Action : RadioActionBase {
    public Radio2Action(IScene scene) : base(scene, "Assets/087-B/Sounds/radio2.wav") { }
}
