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

public class GasMask : IItem<GasMask>, IEntityHolder {
    private readonly GraphicsResources _gfxRes;
    private readonly IScene _scene;
    private readonly Prop _prop;

    private bool _picked = false;

    public Vector3 Position => _picked ? Vector3.Zero : _prop.Models.WorldTransform.Position;

    public GasMask(GraphicsResources gfxRes, PhysicsResources physics, IScene scene, Transform transform) {
        _gfxRes = gfxRes;
        _scene = scene;
        _prop = new(physics, "Assets/Items/gasmask.b3d", transform, false);
        InventoryIcon = gfxRes.TextureCache.GetTexture("Assets/Textures/InventoryIcons/INVgasmask.jpg");
    }

    public static GasMask Create(GraphicsResources gfxRes, PhysicsResources physics, IScene scene, Transform transform) {
        return new(gfxRes, physics, scene, transform);
    }

    public string DisplayName => "Gas Mask";

    public ICBTexture InventoryIcon { get; }

    public bool CanBePicked(Player player) => !_picked;

    public void OnPicked(Player player) {
        Debug.Assert(!_picked);

        if (player.PickItem(this)) {
            _picked = true;
            _scene.MoveEntity(_prop);
        }
    }

    public ICBTexture GetHandTexture() => _gfxRes.TextureCache.GetTexture("Assets/Textures/HUD/handsymbol.png");

    public IEnumerable<IEntity> Entities {
        get {
            yield return _prop;
        }
    }

    public void OnUsed() {
        Log.Warning("Equipped gas mask!");
    }
}
