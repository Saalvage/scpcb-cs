namespace SCPCB.B.Actions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ActInfoAttribute : Attribute;

public class FixedFloorActionInfoAttribute : ActInfoAttribute {
    public int MinFloor { get; }
    public int MaxFloor { get; }
    public float Probability { get; }

    public FixedFloorActionInfoAttribute(int floor, float probability = 1f) {
        MinFloor = MaxFloor = floor;
        Probability = probability;
    }

    public FixedFloorActionInfoAttribute(int minFloor, int maxFloor, float probability = 1f) {
        MinFloor = minFloor;
        MaxFloor = maxFloor;
        Probability = probability;
    }
}

public class RandomFloorActionInfoAttribute : ActInfoAttribute {
    public int MinFloor { get; }
    public int MaxFloor { get; }
    public int Weight { get; }

    public RandomFloorActionInfoAttribute(int minFloor, int maxFloor, int weight = 1) {
        MinFloor = minFloor;
        MaxFloor = maxFloor;
        Weight = weight;
    }
}
