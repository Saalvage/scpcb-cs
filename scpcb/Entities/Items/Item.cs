using scpcb.Graphics;
using scpcb.Graphics.Primitives;

namespace scpcb.Entities.Items;

public interface IItem : IEntity {
    string DisplayName => GetType().AssemblyQualifiedName!;
    ICBTexture InventoryIcon { get; }
}

public interface IItem<out T> : IItem where T : IItem<T> {
    static abstract T Create(GraphicsResources gfxRes);
}
