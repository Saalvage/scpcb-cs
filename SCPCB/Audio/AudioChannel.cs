using OpenTK.Audio.OpenAL;
using SCPCB.Utility;

namespace SCPCB.Audio;

// TODO: The current design is likely sufficient for the foreseeable future, but a proper system would likely
// be similar to the IConstantProviders system for shaders. This would allow for having one sound
// playback be affected by multiple sources (e.g. entity position and a music volume option).
public class AudioChannel : Disposable {
    protected readonly int _source = AL.GenSource();

    public bool IsPlaying => (ALSourceState)AL.GetSource(_source, ALGetSourcei.SourceState) == ALSourceState.Playing;

    public bool IsLooping {
        get => AL.GetSource(_source, ALSourceb.Looping);
        set => AL.Source(_source, ALSourceb.Looping, value);
    }

    public float Time {
        get => AL.GetSource(_source, ALSourcef.SecOffset);
        set => AL.Source(_source, ALSourcef.SecOffset, value);
    }

    public void Play(AudioFile file) {
        AL.Source(_source, ALSourcei.Buffer, file.BufferHandle);
        AL.SourcePlay(_source);
    }

    public void Pause(AudioFile file) {
        AL.SourcePause(_source);
    }

    public void Stop(AudioFile file) {
        AL.SourceStop(_source);
    }

    protected override void DisposeImpl() {
        AL.DeleteSource(_source);
    }
}
