using System.Numerics;

namespace scpcb.Entities;

public interface I3DEntity : IEntity {
    Vector3 Position { get; }
}
