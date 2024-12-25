namespace SCPCB.Utility; 

public abstract class Disposable : IDisposable {
    private bool _disposed = false;

    ~Disposable() {
        Dispose();
    }

    public void Dispose() {
        if (_disposed) { return; }
        _disposed = true;
        GC.SuppressFinalize(this);
        DisposeImpl();
    }

    protected abstract void DisposeImpl();
}
