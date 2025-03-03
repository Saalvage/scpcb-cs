using System.Diagnostics;
using NAudio.Wave;
using OpenTK.Audio.OpenAL;
using SCPCB.Utility;

namespace SCPCB.Audio;

public enum Channels {
    Mono = 1,
    Stereo = 2,
}

public class AudioFile : Disposable {
    public int BufferHandle { get; }

    public Channels ChannelCount { get; }

    public AudioFile(string path, Channels? convertChannels = null) {
        BufferHandle = AL.GenBuffer();
        // TODO: This in a better way.
        using var reader = new AudioFileReader(path);
        Debug.Assert(reader.WaveFormat.Channels <= 2);
        var wasMono = reader.WaveFormat.Channels == 1;
        var isMono = convertChannels switch {
            null => wasMono,
            Channels.Mono => true,
            Channels.Stereo => false,
        };
        ChannelCount = isMono ? Channels.Mono : Channels.Stereo;
        var sampleProvider = wasMono == isMono
            ? reader
            : (isMono ? reader.ToMono() : reader.ToStereo());
        var buffer = new byte[(reader.Length / reader.WaveFormat.BitsPerSample) * 16
                              / (isMono && !wasMono ? 2 : 1)
                              * (!isMono && wasMono ? 2 : 1)];
        sampleProvider.ToWaveProvider16().Read(buffer, 0, buffer.Length);
        AL.BufferData(BufferHandle, isMono ? ALFormat.Mono16 : ALFormat.Stereo16, buffer, reader.WaveFormat.SampleRate);
    }

    protected override void DisposeImpl() {
        AL.DeleteBuffer(BufferHandle);
    }
}
