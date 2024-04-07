using scpcb.Graphics;
using scpcb.Graphics.Primitives;
using scpcb.Physics;
using scpcb.Scenes;
using scpcb.Utility;

namespace scpcb.Entities.Items;

public interface IItem : IPickableEntity {
    string DisplayName => GetType().FullName!;
    ICBTexture InventoryIcon { get; }
    void OnUsed() { }
}

public interface IItem<out T> : IItem where T : IItem<T> {
    static abstract T Create(GraphicsResources gfxRes, PhysicsResources physics, IScene scene, Transform transform);
}
