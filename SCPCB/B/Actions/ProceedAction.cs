using SCPCB.Audio;
using SCPCB.Entities;
using SCPCB.Scenes;

namespace SCPCB.B.Actions;

[FixedFloorActionInfo(0)]
public class ProceedAction : IFloorAction, ITickable {
    public string PredeterminedFloor => "map0";

    private readonly AudioResources _audio;
    private readonly AudioFile _radio;
    private readonly AudioChannel _radioChannel = new();

    private bool _active = false;
    private bool _done = false;
    private int _counter = 0;

    public ProceedAction(IScene scene) {
        _audio = scene.Audio;
        _radio = _audio.SoundCache.GetSound("Assets/087-B/Sounds/radio1.wav");
    }

    public void OnEnter() {
        _active = true;
    }

    public void Tick() {
        if (!_done && _active) {
            _counter++;
            
            if (_counter >= 150) {
                _radioChannel.Play(_radio);
                _done = true;
            }
        }
    }
}
