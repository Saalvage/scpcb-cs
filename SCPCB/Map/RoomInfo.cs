using System.Text.Json.Serialization;

namespace SCPCB.Map;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Shape {
    _1,
    _2,
    _2C,
    _3,
    _4,
}

public record RoomInfo(string Name, string? Description, string Mesh, Shape Shape);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Direction {
    Up,
    Right,
    Down,
    Left,
}

public static class DirectionExtensions {
    public static float ToDegrees(this Direction dir) => dir switch {
        Direction.Up => 0,
        Direction.Right => 90,
        Direction.Down => 180,
        Direction.Left => 270,
    };

    public static float ToRadians(this Direction dir) => dir.ToDegrees() * MathF.PI / 180;

    public static Direction Rotate(this Direction dir, int turns) => (Direction)(((int)dir + turns + 4) % 4);
    public static Direction Rotate(this Direction dir, Direction turns) => dir.Rotate((int)turns);

    public static bool HasOpening(this PlacedRoomInfo room, Direction intoDirection) {
        var baseDir = intoDirection.Rotate(-(int)room.Direction);
        return room.Room.Shape switch {
            Shape._1 => baseDir is Direction.Down,
            Shape._2 => baseDir is Direction.Down or Direction.Up,
            Shape._2C => baseDir is Direction.Down or Direction.Right,
            Shape._3 => baseDir is not Direction.Up,
            Shape._4 => true,
        };
    }
}

public record PlacedRoomInfo(RoomInfo Room, Direction Direction);
