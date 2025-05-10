using SCPCB.Audio.Properties;

namespace SCPCB.Audio;

public static class AudioExtensions {
    public static Playback Play(this AudioResources audio, string file, params ReadOnlySpan<IAudioProperty> properties)
        => audio.Play(audio.SoundCache.GetSound(file), properties);

    public static Playback Play(this AudioResources audio, string file, Channels? channels, params ReadOnlySpan<IAudioProperty> properties)
        => audio.Play(audio.SoundCache.GetSound(file, channels), properties);

    public static void PlayFireAndForget(this AudioResources audio, string file, params ReadOnlySpan<IAudioProperty> properties)
        => audio.PlayFireAndForget(audio.SoundCache.GetSound(file), properties);

    public static void PlayFireAndForget(this AudioResources audio, string file, Channels? channels, params ReadOnlySpan<IAudioProperty> properties)
        => audio.PlayFireAndForget(audio.SoundCache.GetSound(file, channels), properties);
}
