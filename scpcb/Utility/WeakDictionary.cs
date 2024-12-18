using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace scpcb.Utility; 

public class WeakDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TValue : class {
    private readonly Dictionary<TKey, WeakReference<TValue>> _dict = [];

    private void CleanUp() {
        foreach (var (key, value) in _dict) {
            if (!value.TryGetTarget(out _)) {
                _dict.Remove(key);
            }
        }
    }

    private bool TryGetValueCleaning(TKey key, [MaybeNullWhen(false)] out TValue value) {
        if (_dict.TryGetValue(key, out var weak)) {
            if (weak.TryGetTarget(out var val)) {
                value = val;
                return true;
            } else {
                _dict.Remove(key);
            }
        }

        value = null;
        return false;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
        foreach (var item in _dict) {
            if (item.Value.TryGetTarget(out var val)) {
                yield return new(item.Key, val);
            } else {
                _dict.Remove(item.Key);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public void Add(KeyValuePair<TKey, TValue> item) {
        if (_dict.TryGetValue(item.Key, out var weak) && !weak.TryGetTarget(out _)) {
            weak.SetTarget(item.Value);
        } else {
            _dict.Add(item.Key, new(item.Value));
        }
    }

    public void Clear() => _dict.Clear();

    public bool Contains(KeyValuePair<TKey, TValue> item) => TryGetValueCleaning(item.Key, out var val) && val == item.Value;

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
        foreach (var item in this) {
            array[arrayIndex++] = item;
        }
    }

    public bool Remove(KeyValuePair<TKey, TValue> item) => Contains(item) && _dict.Remove(item.Key);

    public int Count {
        get {
            CleanUp();
            return _dict.Count;
        }
    }

    public bool IsReadOnly => false;

    public void Add(TKey key, TValue value) {
        if (_dict.TryGetValue(key, out var weak) && !weak.TryGetTarget(out _)) {
            weak.SetTarget(value);
        } else {
            _dict.Add(key, new(value));
        }
    }

    public bool ContainsKey(TKey key) => TryGetValueCleaning(key, out _);

    public bool Remove(TKey key) {
        TryGetValueCleaning(key, out _);
        return _dict.Remove(key);
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        => TryGetValueCleaning(key, out value);

    public TValue this[TKey key] {
        get => TryGetValueCleaning(key, out var value)
            ? value
            : throw new KeyNotFoundException($"{key} does not exist in the weak dictionary!");
        set {
            if (_dict.TryGetValue(key, out var weak)) {
                weak.SetTarget(value);
            } else {
                _dict.Add(key, new(value));
            }
        }
    }

    // We need to do the sweep to make sure that Values does not return fewer items than Keys does.
    public ICollection<TKey> Keys {
        get {
            CleanUp();
            return _dict.Keys;
        }
    }

    /// <summary>
    /// It is recommended not to call this because it is forced to create a new list.
    /// </summary>
    // Reason being that values could be collected between a removing iteration and the return.
    public ICollection<TValue> Values
        => this.Select(x => x.Value).ToList();
}
