using System.Diagnostics;
using System.Numerics;
using SCPCB.Graphics.Primitives;
using SCPCB.Map.Entities;
using SCPCB.Physics;
using SCPCB.PlayerController;
using SCPCB.Scenes;
using SCPCB.Utility;

namespace SCPCB.Entities.Items;

public interface IItem : IPickableEntity {
    string DisplayName => GetType().FullName!;
    ICBTexture InventoryIcon { get; }
    void OnUsed() { }
    void OnDropped() { }
}

public class Item : IItem, IEntityHolder {
    protected readonly IScene _scene;
    // TODO: Make this work with serialization.
    protected readonly Prop _prop;

    private bool _picked = false;

    public ICBTexture InventoryIcon { get; }

    public Vector3 Position => _picked ? Vector3.Zero : _prop.Models.WorldTransform.Position;

    public Item(IScene scene, ICBTexture inventoryIcon, string modelFile, Transform transform) {
        _scene = scene;
        _prop = new(scene.GetEntitiesOfType<PhysicsResources>().First(), modelFile, transform, false);
        InventoryIcon = inventoryIcon;
    }

    public Item(IScene scene, string inventoryIconFile, string modelFile, Transform transform)
        : this(scene, scene.Graphics.TextureCache.GetTexture(inventoryIconFile), modelFile, transform) { }

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

    public virtual ICBTexture GetHandTexture() => _scene.Graphics.TextureCache.GetTexture("Assets/Textures/HUD/handsymbol.png");

    public virtual IEnumerable<IEntity> Entities {
        get {
            yield return _prop;
        }
    }
}
