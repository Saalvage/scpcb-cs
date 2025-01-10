using SCPCB.Graphics.Primitives;
using SCPCB.PlayerController;

namespace SCPCB.Entities;

public interface IPickableEntity : IPositionalEntity {
    bool CanBePicked(Player player) => true;
    void OnPicked(Player player);
    ICBTexture GetHandTexture();
}
