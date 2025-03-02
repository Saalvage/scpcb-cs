using SCPCB.Graphics.Primitives;
using SCPCB.PlayerController;
using SCPCB.Utility;

namespace SCPCB.Entities;

public interface IPickableEntity : IEntity, IPositioned {
    bool CanBePicked(Player player) => true;
    void OnPicked(Player player);
    ICBTexture GetHandTexture();
}
