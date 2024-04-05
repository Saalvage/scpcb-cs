using scpcb.Graphics;
using scpcb.Physics;
using scpcb.Scenes;
using scpcb.Utility;

namespace scpcb.Entities.Items;

public class ItemManager {
    private readonly GraphicsResources _gfxRes;
    private readonly PhysicsResources _physics;
    private readonly IScene _scene;

    public ItemManager(GraphicsResources gfxRes, PhysicsResources physics, IScene scene) {
        _gfxRes = gfxRes;
        _physics = physics;
        _scene = scene;
    }

    public IItem CreateItem(string className, Transform transform) {
        var allTypes = Helpers.GetAllLoadedTypes().ToArray();
        var itemType = allTypes.SingleOrDefault(x => x.Name == className);
        itemType ??= allTypes.SingleOrDefault(x => x.FullName == className);
        itemType ??= allTypes.SingleOrDefault(x => x.AssemblyQualifiedName == className);
        var method = typeof(ItemManager).GetMethods().Single(x => x.IsGenericMethod && x.Name == "CreateItem");
        return (IItem)method.MakeGenericMethod(itemType).Invoke(this, [transform]);
    }

    public T CreateItem<T>(Transform transform) where T : IItem<T> {
        return T.Create(_gfxRes, _physics, _scene, transform);
    }
}
