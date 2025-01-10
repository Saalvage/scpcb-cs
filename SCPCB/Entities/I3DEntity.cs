using System.Numerics;
using SCPCB.Utility;

namespace SCPCB.Entities;

public interface I3DEntity : IPositionalEntity {
    Vector3 IPositionalEntity.Position => WorldTransform.Position;
    Transform WorldTransform { get; set; }
}
