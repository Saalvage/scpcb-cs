using System.Numerics;

namespace SCPCB.Entities;

public interface IPositionalEntity : IEntity {
    Vector3 Position { get; }
}
