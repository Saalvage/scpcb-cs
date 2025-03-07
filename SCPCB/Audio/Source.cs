using OpenTK.Audio.OpenAL;
using SCPCB.Utility;

namespace SCPCB.Audio;

public class Source : Disposable, ISource {
    public int Handle { get; }

    // Keep alive.
    private readonly AudioFile _file;

    private WeakReference<Playback>? _playback;

    private static int _sourceCount = 0;
    public static int ActiveSources => _sourceCount;

    private Source(AudioFile file) {
        Interlocked.Increment(ref _sourceCount);
        Handle = AL.GenSource();
        AL.Source(Handle, ALSourcei.Buffer, file.BufferHandle);
    }

    public static (Source, Playback) CreateWithPlayback(AudioFile file) {
        var source = Create(file);
        var playback = new Playback(source);
        source._playback = new(playback);
        return (source, playback);
    }

    public static Source Create(AudioFile file) => new(file);

    // Mark the playback handle as no longer being in use which may allow for
    // destroying the source (if it is not playing).
    public void MarkHandleFinished() {
        _playback = null;
        TryDispose();
    }

    public bool TryDispose() {
        if (IsDisposed) {
            return true;
        }

        if (_playback?.IsAlive() is true
            || (ALSourceState)AL.GetSource(Handle, ALGetSourcei.SourceState) == ALSourceState.Playing) {
            return false;
        }

        Dispose();
        return true;
    }

    protected override void DisposeImpl() {
        AL.DeleteSource(Handle);
        Interlocked.Decrement(ref _sourceCount);
    }
}
