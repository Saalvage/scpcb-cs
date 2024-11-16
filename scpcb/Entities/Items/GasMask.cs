using System.Diagnostics;
using System.Numerics;
using scpcb.Graphics;
using scpcb.Graphics.Primitives;
using scpcb.Map.Entities;
using scpcb.Physics;
using scpcb.PlayerController;
using scpcb.Scenes;
using scpcb.Utility;

namespace scpcb.Entities.Items;

public class GasMask : Item, IItem<GasMask> {
    public GasMask(GraphicsResources gfxRes, IScene scene, Transform transform)
        : base(gfxRes, scene, "Assets/Textures/InventoryIcons/INVgasmask.jpg", "Assets/Items/gasmask.b3d", transform) { }

    public static GasMask Create(GraphicsResources gfxRes, IScene scene, Transform transform) {
        return new(gfxRes, scene, transform);
    }

    public override void OnUsed() {
        Log.Warning("Equipped gas mask!");
    }
}
