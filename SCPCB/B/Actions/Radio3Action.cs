using SCPCB.Scenes;

namespace SCPCB.B.Actions;

[FixedFloorActionInfo(4, 5, 2 / 3f)]
public class Radio3Action : RadioActionBase {
    public Radio3Action(IScene scene) : base(scene, "Assets/087-B/Sounds/radio3.ogg") { }
}
