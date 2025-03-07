using OpenTK.Audio.OpenAL;
using SCPCB.Audio.Properties;
using SCPCB.Utility;

namespace SCPCB.Audio;

public class AudioResources : Disposable {
    private readonly ALDevice _device;
    private readonly ALContext _ctx;

    public SoundCache SoundCache { get; } = new();

    private readonly SourceCollection _sources = [];

    public static IEnumerable<string> GetDevices() {
        if (ALC.IsEnumerationExtensionPresent(ALDevice.Null)) {
            return ALC.GetStringList(GetEnumerationStringList.DeviceSpecifier);
        }
        return "".AsEnumerableElementOrEmpty();
    }

    public AudioResources(string? device = null) {
        _device = ALC.OpenDevice(device ?? "");
        // TODO: By default OpenAL creates its own thread, do we want this?
        _ctx = ALC.CreateContext(_device, (int[]?)null);
        ALC.MakeContextCurrent(_ctx);
    }

    // Amount of ticks between cleanups.
    private const int TICK_COUNT = 60;
    private int _tickCount;

    public void Tick() {
        if (_tickCount++ % TICK_COUNT == 0) {
            _sources.CleanUp();
        }
    }

    private void PrepareSource(Source source, ReadOnlySpan<IAudioProperty> properties) {
        // TODO: Caching of sources?
        // The issue is that we would need to reset all properties before being able to reuse a source.
        // Is this even an issue? How expensive is the generation of a source?
        // Another issue is the need for a distinction between a source and a "strong" playback.
        _sources.Add(source);
        // TODO: Better way to represent defaults?
        AL.Source(source.Handle, ALSourceb.SourceRelative, true);
        foreach (var p in properties) {
            p.Apply(source);
        }
        source.Play();
    }

    // TODO: For the time being we've given up on the concept of channels.
    // However, they have merit and should likely return in some form.
    // They provide grouping of sounds by one category (e.g. music and sound effects) which is useful
    // for volume controls as well as collective pausing operations.
    public Playback Play(AudioFile file, params ReadOnlySpan<IAudioProperty> properties) {
        var (source, playback) = Source.CreateWithPlayback(file);
        PrepareSource(source, properties);
        return playback;
    }

    public void PlayFireAndForget(AudioFile file, params ReadOnlySpan<IAudioProperty> properties) {
        var source = Source.Create(file);
        PrepareSource(source, properties);
    }

    protected override void DisposeImpl() {
        ALC.CloseDevice(_device);
        ALC.DestroyContext(_ctx);
        foreach (var s in _sources) {
            s.Dispose();
        }
    }
}
