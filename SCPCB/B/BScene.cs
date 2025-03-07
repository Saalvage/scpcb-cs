using System.Numerics;
using SCPCB.Audio;
using SCPCB.Audio.Properties;
using SCPCB.Scenes;
using SCPCB.Utility;

namespace SCPCB.B;

public class BScene : MainScene {
    private readonly AudioFile[] _ambient;

    public BScene(Game game) : base(game, new(1f, 0.3f, 1.8f) {
        CameraOffset = 0.303f,
    }) {
        var music = Audio.SoundCache.GetSound("Assets/087-B/Sounds/music.ogg");
        Audio.PlayFireAndForget(music, new LoopingAudioProperty());

        _ambient = Enumerable.Range(1, 8)
            .Select(x => Audio.SoundCache.GetSound($"Assets/087-B/Sounds/ambient{x}.ogg", Channels.Mono))
            .ToArray();

        new MapGenerator(this).InstantiateNewMap(210);
    }

    public override void Tick() {
        base.Tick();
        if (Random.Shared.Next(1000) == 0) {
            Audio.PlayFireAndForget(_ambient.RandomElement(), new StaticAudioTransformProperty(
                _player.Camera.WorldTransform.Position + new Vector3(
                    Random.Shared.NextSingle(-1, 1),
                    Random.Shared.NextSingle(-2, -10),
                    Random.Shared.NextSingle(-1, 1)
                )));
        }
    }
}
