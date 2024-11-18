using scpcb.Graphics;
using scpcb.Scenes;
using scpcb.Utility;

namespace scpcb.Entities.Items;

public class GasMask : Item {
    public GasMask(GraphicsResources gfxRes, IScene scene, Transform transform)
        : base(gfxRes, scene, "Assets/Textures/InventoryIcons/INVgasmask.jpg", "Assets/Items/gasmask.b3d", transform) { }

    public override void OnUsed() {
        Log.Warning("Equipped gas mask!");
    }
}
