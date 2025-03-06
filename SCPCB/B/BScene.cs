using SCPCB.Audio;
using SCPCB.Scenes;

namespace SCPCB.B;

public class BScene : MainScene {
    private readonly AudioChannel _musicChannel = new() { IsLooping = true };

    public BScene(Game game) : base(game, new(1f, 0.3f, 1.8f) {
        CameraOffset = 0.303f,
    }) {
        var music = Audio.SoundCache.GetSound("Assets/087-B/Sounds/music.ogg");
        _musicChannel.Play(music);
        new MapGenerator(this).InstantiateNewMap(210);
    }

    public override void OnLeave() {
        _musicChannel.Dispose();
        base.OnLeave();
    }
}
