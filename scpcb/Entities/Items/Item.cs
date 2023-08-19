namespace scpcb.Entities.Items;

public interface IItem {
    public string DisplayName => GetType().AssemblyQualifiedName ?? GetType().FullName ?? GetType().Name;
}

public class Item : IItem {
    public string DisplayName { get; }
}
