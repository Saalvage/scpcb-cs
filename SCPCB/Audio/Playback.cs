using OpenTK.Audio.OpenAL;
using SCPCB.Utility;

namespace SCPCB.Audio;

public class Playback : Disposable, ISource {
    private readonly Source _playback;

    int ISource.Handle => _playback.Handle;

    public bool IsPlaying => (ALSourceState)AL.GetSource(_playback.Handle, ALGetSourcei.SourceState) == ALSourceState.Playing;

    public float Time {
        get => AL.GetSource(_playback.Handle, ALSourcef.SecOffset);
        set => AL.Source(_playback.Handle, ALSourcef.SecOffset, value);
    }

    public Playback(Source playback) {
        _playback = playback;
    }

    protected override void DisposeImpl() {
        _playback.MarkHandleFinished();
    }
}
