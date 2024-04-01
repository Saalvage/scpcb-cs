using scpcb.Graphics;
using scpcb.Graphics.Primitives;

namespace scpcb.Entities.Items;

public class GasMask : IItem<GasMask> {
    public GasMask(GraphicsResources gfxRes) {
        InventoryIcon = gfxRes.TextureCache.GetTexture("Assets/Textures/InventoryIcons/INVgasmask.jpg");
    }

    public static GasMask Create(GraphicsResources gfxRes) {
        return new(gfxRes);
    }

    public string DisplayName => "Gas Mask";

    public ICBTexture InventoryIcon { get; }
}
