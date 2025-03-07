using SCPCB.Audio;
using SCPCB.Entities;
using SCPCB.Scenes;

namespace SCPCB.B.Actions;

public abstract class RadioActionBase : IFloorAction, ITickable {
    public virtual string PredeterminedFloor => "map";

    private readonly AudioResources _audio;
    private readonly AudioFile _radio;

    private readonly int _delayInTicks;

    private bool _active = false;
    private bool _done = false;
    private int _counter = 0;

    protected RadioActionBase(IScene scene, string soundPath, int delayTicks = 0) {
        _audio = scene.Audio;
        _radio = _audio.SoundCache.GetSound(soundPath);
        _delayInTicks = delayTicks;
    }

    public void OnEnter() {
        _active = true;
    }

    public void Tick() {
        if (!_done && _active) {
            _counter++;

            if (_counter >= _delayInTicks) {
                _audio.PlayFireAndForget(_radio);
                _done = true;
            }
        }
    }
}
