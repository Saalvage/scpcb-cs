using System.Numerics;

namespace SCPCB.B;

public static class BHelpers {
    public static int GetFloor(Vector3 pos) => (int)((-pos.Y - 0.5f) / 2);

    public static Vector3 GetFloorCenter(int floor) => new(4, -1 - floor * 2, floor % 2 == 0 ? 0.5f : 6.5f);
    public static Vector3 GetFloorStart(int floor) => GetFloorCenter(floor) with { X = floor % 2 == 0 ? 8f : 0f };
    public static Vector3 GetFloorEnd(int floor) => GetFloorCenter(floor) with { X = floor % 2 == 0 ? 0f : 8f };
}
