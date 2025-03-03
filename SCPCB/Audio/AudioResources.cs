using OpenTK.Audio.OpenAL;
using SCPCB.Utility;

namespace SCPCB.Audio;

public class AudioResources : Disposable {
    private readonly ALDevice _device;
    private readonly ALContext _ctx;

    public SoundCache SoundCache { get; } = new();

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

    protected override void DisposeImpl() {
        ALC.CloseDevice(_device);
        ALC.DestroyContext(_ctx);
    }
}
