using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;

namespace scpcb.Physics; 

public class PhysicsResources {
    public Simulation Simulation { get; }

    public BufferPool BufferPool { get; } = new();
    
    private readonly ThreadDispatcher ThreadDispatcher;

    // TODO vvv Remove vvv
    public List<BodyReference> Bodies { get; } = new();

    public static Box Box { get; } = new(1, 1, 1);
    public static BodyInertia BoxInertia { get; } = Box.ComputeInertia(1);

    public TypedIndex BoxIndex { get; }
    // TODO ^^^ Remove ^^^

    public PhysicsResources() {
        Simulation = Simulation.Create(BufferPool, new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(), new(4, 2));

        var targetThreadCount = int.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
        ThreadDispatcher = new(targetThreadCount);

        BoxIndex = Simulation.Shapes.Add(Box);
        const int pyramidCount = 5;
        for (var pyramidIndex = 0; pyramidIndex < pyramidCount; ++pyramidIndex) {
            const int rowCount = 20;
            for (var rowIndex = 0; rowIndex < rowCount; ++rowIndex) {
                int columnCount = rowCount - rowIndex;
                for (var columnIndex = 0; columnIndex < columnCount; ++columnIndex) {
                    var added = Simulation.Bodies.Add(BodyDescription.CreateDynamic(new Vector3(
                            (-columnCount * 0.5f + columnIndex) * Box.Width,
                            (rowIndex + 0.5f) * Box.Height,
                            (pyramidIndex - pyramidCount * 0.5f) * (Box.Length + 4) - 20),
                        BoxInertia, BoxIndex, 0.01f));
                    Bodies.Add(Simulation.Bodies.GetBodyReference(added));
                }
            }
        }

        Simulation.Statics.Add(new(new Vector3(0, -0.5f, 0), Simulation.Shapes.Add(new Box(2500, 1, 2500))));
    }

    public void Update(float delta) {
        Simulation.Timestep(delta * 10, ThreadDispatcher);
    }
}
