using System.Numerics;
using SCPCB.Map;

namespace SCPCB.Scenes;

public class CBScene : MainScene {
    private readonly Dictionary<string, IRoomData> _rooms;

    public CBScene(Game game, PlacedRoomInfo?[,] map) : base(game, new(1.5f, 0.25f, 2.5f)) {
        _rooms = map.Cast<PlacedRoomInfo?>()
            .Where(x => x != null)
            .Select(x => x.Room.Mesh)
            .Distinct()
            .ToDictionary(x => x, x => Graphics.LoadRoom(this, Physics, "Assets/Rooms/" + x));
        for (var x = 0; x < map.GetLength(0); x++) {
            for (var y = 0; y < map.GetLength(1); y++) {
                var info = map[x, y];
                if (info != null) {
                    var room = _rooms[info.Room.Mesh].Instantiate(new(x * 20.5f, 0, y * 20.5f),
                        Quaternion.CreateFromYawPitchRoll(-info.Direction.ToRadians() + MathF.PI, 0, 0));
                    AddEntity(room);
                }
            }
        }
    }

    protected override void DisposeImpl() {
        foreach (var room in _rooms.Values) {
            room.Dispose();
        }
        base.DisposeImpl();
    }
}
