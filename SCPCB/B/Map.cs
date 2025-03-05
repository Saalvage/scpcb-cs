using SCPCB.B.Actions;
using SCPCB.Entities;
using SCPCB.PlayerController;
using SCPCB.Scenes;

namespace SCPCB.B;

public class Map : ITickable {
    private readonly Player _player;

    private readonly IFloorAction?[] _acts;

    private int _prevFloor = -1;

    public Map(IScene scene, IFloorAction?[] acts) {
        _player = scene.GetEntitiesOfType<Player>().Single();
        _acts = acts;
    }

    public void Tick() {
        var playerFloor = BHelpers.GetFloor(_player.Camera.WorldTransform.Position);
        if (_prevFloor != playerFloor) {
            if (_prevFloor >= 0 && _prevFloor < _acts.Length) {
                _acts[_prevFloor]?.OnLeave();
            }
            if (playerFloor >= 0 && playerFloor < _acts.Length) {
                _acts[playerFloor]?.OnEnter();
            }
            _prevFloor = playerFloor;
        }
    }
}
