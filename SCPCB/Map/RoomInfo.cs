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
    Down,
    Left,
    Right,
}

public static class DirectionExtensions {
    public static float ToDegrees(this Direction dir) => dir switch {
        Direction.Up => 0,
        Direction.Down => 180,
        Direction.Left => 90,
        Direction.Right => -90,
    };

    public static float ToRadians(this Direction dir) => dir.ToDegrees() * MathF.PI / 180;
}

public record PlacedRoomInfo(RoomInfo Room, Direction Direction);
