using SCPCB.Scenes;
using SCPCB.Utility;

namespace SCPCB.Entities.Items;

public class GasMask : Item {
    public GasMask(IScene scene, Transform transform)
        : base(scene, "Assets/Textures/InventoryIcons/INVgasmask.jpg", "Assets/Items/gasmask.b3d", transform) { }

    public override void OnUsed() {
        Log.Warning("Equipped gas mask!");
    }
}
