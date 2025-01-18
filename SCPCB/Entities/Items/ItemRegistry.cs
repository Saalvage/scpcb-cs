using System.ComponentModel;
using System.Globalization;
using SCPCB.Graphics;
using SCPCB.Scenes;
using SCPCB.Utility;

namespace SCPCB.Entities.Items;

public class ItemRegistry(IScene scene) {
    private readonly Dictionary<string, Func<Transform, IItem>> _templates = [];

    public void RegisterItem(string name, Func<Transform, IItem> template) {
        _templates[name] = template;
    }

    public void RegisterItemsFromFile(string filename) {
        foreach (var line in File.ReadLines(filename)) {
            if (line.Length == 0) { continue; }
            var split = line.Split(';');
            var type = Helpers.GetAllLoadedTypes().First(x => x.FullName == split[1]);
            var (ctor, @params) = type.GetConstructors()
                .Where(x => x.GetParameters().Length - 2 == split.Length - 2)
                .Select(x => (x, @params: x.GetParameters()
                    .Skip(2)
                    .Select(x => TypeDescriptor.GetConverter(x.ParameterType))
                    .Zip(split.Skip(2))))
                .First(x => x.@params.All(x => x.First.IsValid(x.Second)));
            RegisterItem(split[0], transform => (IItem)ctor.Invoke(new object[]{scene, transform}
                .Concat(@params.Select(x => x.First.ConvertFromString(null, CultureInfo.InvariantCulture, x.Second)))
                .ToArray()));
        }
    }

    public IItem CreateItem(Transform transform, string name) {
        return _templates[name].Invoke(transform);
    }
}
