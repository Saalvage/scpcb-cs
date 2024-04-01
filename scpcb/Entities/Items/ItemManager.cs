using scpcb.Graphics;
using scpcb.Utility;

namespace scpcb.Entities.Items;

public class ItemManager {
    private readonly GraphicsResources _gfxRes;

    public ItemManager(GraphicsResources gfxRes) {
        _gfxRes = gfxRes;
    }

    public IItem CreateItem(string className) {
        var allTypes = Helpers.GetAllLoadedTypes().ToArray();
        var itemType = allTypes.SingleOrDefault(x => x.Name == className);
        itemType ??= allTypes.SingleOrDefault(x => x.FullName == className);
        itemType ??= allTypes.SingleOrDefault(x => x.AssemblyQualifiedName == className);
        var method = typeof(ItemManager).GetMethods().Single(x => x.IsGenericMethod && x.Name == "CreateItem");
        return (IItem)method.MakeGenericMethod(itemType).Invoke(this, null);
    }

    public T CreateItem<T>() where T : IItem<T> {
        return T.Create(_gfxRes);
    }
}
