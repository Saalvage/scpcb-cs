using System.Numerics;

namespace SCPCB.B;

public static class BHelpers {
    public static int GetFloor(Vector3 pos) {
        return (int)((-pos.Y - 0.5f) / 2);
    }
}
