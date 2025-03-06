using System.Diagnostics;
using NAudio.Wave;
using NVorbis;
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
        // TODO: This in a better way.
        byte[] buffer;
        bool isMono;
        int freq;
        if (path.EndsWith(".ogg", StringComparison.InvariantCultureIgnoreCase)) {
            using var reader = new NVorbisReader(path);
            freq = reader.WaveFormat.SampleRate;
            (buffer, isMono) = LoadBuffer(reader, reader.Length);
        } else {
            using var reader = new AudioFileReader(path);
            freq = reader.WaveFormat.SampleRate;
            (buffer, isMono) = LoadBuffer(reader, reader.Length);
        }

        ChannelCount = isMono ? Channels.Mono : Channels.Stereo;

        BufferHandle = AL.GenBuffer();
        AL.BufferData(BufferHandle, isMono ? ALFormat.Mono16 : ALFormat.Stereo16, buffer, freq);

        (byte[], bool) LoadBuffer(ISampleProvider reader, long length) {
            Debug.Assert(reader.WaveFormat.Channels <= 2);
            var wasMono = reader.WaveFormat.Channels == 1;
            var isMono = convertChannels switch {
                null => wasMono,
                Channels.Mono => true,
                Channels.Stereo => false,
            };
            var sampleProvider = wasMono == isMono
                ? reader
                : (isMono ? reader.ToMono() : reader.ToStereo());
            var buffer = new byte[(length / reader.WaveFormat.BitsPerSample) * 16
                                  / (isMono && !wasMono ? 2 : 1)
                                  * (!isMono && wasMono ? 2 : 1)];
            sampleProvider.ToWaveProvider16().Read(buffer, 0, buffer.Length);
            return (buffer, isMono);
        }
    }

    protected override void DisposeImpl() {
        AL.DeleteBuffer(BufferHandle);
    }
}
