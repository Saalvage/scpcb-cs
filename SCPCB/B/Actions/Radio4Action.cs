using SCPCB.Scenes;

namespace SCPCB.B.Actions;

[FixedFloorActionInfo(7, 8, 0.5f)]
public class Radio4Action : RadioActionBase {
    public Radio4Action(IScene scene) : base(scene, "Assets/087-B/Sounds/radio4.ogg") { }
}
