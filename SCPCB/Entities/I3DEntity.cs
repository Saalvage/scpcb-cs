using System.Numerics;

namespace SCPCB.Entities;

public interface I3DEntity : IEntity {
    Vector3 Position { get; }
}
