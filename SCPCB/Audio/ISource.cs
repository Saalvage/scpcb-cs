using OpenTK.Audio.OpenAL;

namespace SCPCB.Audio;

public interface ISource {
    int Handle { get; }
}

public static class SourceExtensions {
    public static void Play(this ISource source) {
        AL.SourcePlay(source.Handle);
    }

    public static void Pause(this ISource source) {
        AL.SourcePause(source.Handle);
    }

    public static void Stop(this ISource source) {
        AL.SourceStop(source.Handle);
    }
}
