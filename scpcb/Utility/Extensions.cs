namespace scpcb.Utility; 

public static class Extensions {
    public static IEnumerable<T> AsEnumerableElementOrEmpty<T>(this T? item) {
        if (item != null) {
            yield return item;
        }
    }

    public static IEnumerable<T> AsEnumerableElement<T>(this T item) {
        yield return item;
    }

    public static void AddRange<T>(this IList<T> list, IEnumerable<T> range) {
        foreach (var item in range) {
            list.Add(item);
        }
    }
}
