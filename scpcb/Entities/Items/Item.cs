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

public interface IItem : IPickableEntity {
    string DisplayName => GetType().FullName!;
    ICBTexture InventoryIcon { get; }
    void OnUsed() { }
    void OnDropped() { }
}

public interface IItem<out T> : IItem where T : IItem<T> {
    static abstract T Create(GraphicsResources gfxRes, IScene scene, Transform transform);
}

public class Item : IItem, IEntityHolder {
    private readonly GraphicsResources _gfxRes;
    private readonly IScene _scene;
    // TODO: Make this work with serialization.
    private readonly Prop _prop;

    private bool _picked = false;

    public ICBTexture InventoryIcon { get; }

    public Vector3 Position => _picked ? Vector3.Zero : _prop.Models.WorldTransform.Position;

    public Item(GraphicsResources gfxRes, IScene scene, ICBTexture inventoryIcon, string modelFile, Transform transform) {
        _gfxRes = gfxRes;
        _scene = scene;
        _prop = new(scene.GetEntitiesOfType<PhysicsResources>().First(), modelFile, transform, false);
        InventoryIcon = inventoryIcon;
    }

    public Item(GraphicsResources gfxRes, IScene scene, string inventoryIconFile, string modelFile, Transform transform)
        : this(gfxRes, scene, gfxRes.TextureCache.GetTexture(inventoryIconFile), modelFile, transform) { }

    public virtual bool CanBePicked(Player player) => !_picked;

    public virtual void OnPicked(Player player) {
        Debug.Assert(!_picked);

        if (player.PickItem(this)) {
            _picked = true;
            _scene.MoveEntity(_prop);
        }
    }

    public virtual void OnUsed() { }
    public virtual void OnDropped() { }

    public virtual ICBTexture GetHandTexture() => _gfxRes.TextureCache.GetTexture("Assets/Textures/HUD/handsymbol.png");

    public virtual IEnumerable<IEntity> Entities {
        get {
            yield return _prop;
        }
    }
}
