using System.Collections;

namespace SCPCB.Audio;

public class SourceCollection : ICollection<Source> {
    private readonly List<Source> _list = [];

    public int Count {
        get {
            CleanUp();
            return _list.Count;
        }
    }

    public bool IsReadOnly => false;

    public void CleanUp() {
        foreach (var _ in this) { }
    }

    public void Add(Source item) => _list.Add(item);
    public bool Remove(Source item) => _list.Remove(item);
    public void Clear() => _list.Clear();
    public bool Contains(Source item) => _list.Contains(item);
    public void CopyTo(Source[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);
    IEnumerator<Source> IEnumerable<Source>.GetEnumerator() => _list.GetEnumerator();

    public IEnumerator<Source> GetEnumerator() {
        for (var i = 0; i < _list.Count; i++) {
            if (_list[i].TryDispose()) {
                _list.RemoveAt(i--);
            } else {
                yield return _list[i];
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
