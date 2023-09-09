using scpcb.Utility;

namespace scpcb.Graphics.Caches; 

public abstract class BaseCache<TKey, TVal> : Disposable where TVal : class, IDisposable {
    protected WeakDictionary<TKey, TVal> _dic = new();

    protected override void DisposeImpl() {
        foreach (var (_, v) in _dic) {
            v.Dispose();
        }
    }
}
