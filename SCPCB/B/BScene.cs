using SCPCB.Scenes;

namespace SCPCB.B;

public class BScene : MainScene {
    public BScene(Game game) : base(game, new(1f, 0.3f, 1.8f) {
        CameraOffset = 0.303f,
    }) {
        new MapGenerator(this).InstantiateNewMap(210);
    }
}
