using OpenTK.Audio.OpenAL;

namespace SCPCB.Audio.Properties;

public class LoopingAudioProperty : IAudioProperty {
    public void Apply(Source playback) {
        AL.Source(playback.Handle, ALSourceb.Looping, true);
    }
}
