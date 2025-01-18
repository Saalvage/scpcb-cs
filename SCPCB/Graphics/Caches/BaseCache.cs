using SCPCB.Utility;

namespace SCPCB.Graphics.Caches; 

public abstract class BaseCache<TKey, TVal> : Disposable where TVal : class, IDisposable where TKey : notnull {
    protected WeakDictionary<TKey, TVal> _dic = [];

    protected override void DisposeImpl() {
        foreach (var (_, v) in _dic) {
            v.Dispose();
        }
    }
}
