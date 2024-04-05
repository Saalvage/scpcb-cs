using System.Collections;

namespace scpcb.Utility;

public class ReadOnlyDuoList<T> : IReadOnlyList<T> {
    private readonly IReadOnlyList<T> _a;
    private readonly IReadOnlyList<T> _b;
    
    public ReadOnlyDuoList(IReadOnlyList<T> a, IReadOnlyList<T> b) {
        _a = a;
        _b = b;
    }

    public IEnumerator<T> GetEnumerator() => _a.Concat(_b).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => _a.Count + _b.Count;

    public T this[int index] => index >= _a.Count ? _b[index - _a.Count] : _a[index];
}
