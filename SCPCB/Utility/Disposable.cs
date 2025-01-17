using System.Diagnostics;

namespace SCPCB.Utility; 

public abstract class Disposable : IDisposable {
    public bool IsDisposed { get; private set; } = false;

#if DEBUG
    // Allows for tracing creation of incorrectly disposed objects.
    private readonly StackTrace _stackTrace = new(true);
#endif

    ~Disposable() {
        Dispose();
    }

    public void Dispose() {
        if (IsDisposed) { return; }
        IsDisposed = true;
        GC.SuppressFinalize(this);
        DisposeImpl();
    }

    protected abstract void DisposeImpl();
}
