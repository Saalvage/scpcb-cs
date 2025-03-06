using SCPCB.Audio;
using SCPCB.Scenes;
using SCPCB.Utility;

namespace SCPCB.B;

public class BScene : MainScene {
    private readonly AudioChannel _musicChannel = new() { IsLooping = true };

    private readonly AudioChannel3D _ambientChannel = new();
    private readonly AudioFile[] _ambient;

    public BScene(Game game) : base(game, new(1f, 0.3f, 1.8f) {
        CameraOffset = 0.303f,
    }) {
        var music = Audio.SoundCache.GetSound("Assets/087-B/Sounds/music.ogg");
        _musicChannel.Play(music);

        _ambient = Enumerable.Range(1, 8)
            .Select(x => new AudioFile($"Assets/087-B/Sounds/ambient{x}.ogg", Channels.Mono))
            .ToArray();

        new MapGenerator(this).InstantiateNewMap(210);
    }

    public override void Tick() {
        base.Tick();
        // TODO: While the sounds overriding one another is accurate to the original, we should improve on it.
        if (Random.Shared.Next(10) == 1000) {
            _ambientChannel.LocalTransform = _player.Camera.WorldTransform + new Transform(new(
                Random.Shared.NextSingle(-1, 1),
                Random.Shared.NextSingle(-2, -10),
                Random.Shared.NextSingle(-1, 1)));
            _ambientChannel.Play(_ambient.RandomElement());
        }
    }

    public override void OnLeave() {
        _musicChannel.Dispose();
        _ambientChannel.Dispose();
        foreach (var sound in _ambient) {
            sound.Dispose();
        }
        base.OnLeave();
    }
}
