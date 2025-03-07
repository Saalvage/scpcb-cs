using NAudio.Wave;
using NVorbis;
using SCPCB.Utility;

namespace SCPCB.Audio;

public class NVorbisReader : Disposable, ISampleProvider {
    private readonly VorbisReader _vorbisReader;

    public WaveFormat WaveFormat { get; }

    public long Length => _vorbisReader.TotalSamples * WaveFormat.BitsPerSample / 8 * WaveFormat.Channels;

    public NVorbisReader(string path) {
        _vorbisReader = new(path);
        WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(_vorbisReader.SampleRate, _vorbisReader.Channels);
    }

    public int Read(float[] buffer, int offset, int count) {
        return _vorbisReader.ReadSamples(buffer.AsSpan()[offset..(offset + count)]);
    }

    protected override void DisposeImpl() {
        _vorbisReader.Dispose();
    }
}
