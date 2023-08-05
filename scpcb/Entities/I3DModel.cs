using System.Numerics;
using scpcb.Graphics;

namespace scpcb.Entities; 

public interface I3DModel {
    Vector3 Position { get; }
    ICBModel Model { get; }
}
