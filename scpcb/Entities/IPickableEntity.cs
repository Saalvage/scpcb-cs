using SCPCB.Graphics.Primitives;
using SCPCB.PlayerController;

namespace SCPCB.Entities;

public interface IPickableEntity : I3DEntity {
    bool CanBePicked(Player player) => true;
    void OnPicked(Player player);
    ICBTexture GetHandTexture();
}
