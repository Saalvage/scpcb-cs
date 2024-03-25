using scpcb.Graphics.Primitives;

namespace scpcb.Entities;

public interface IPickableEntity : I3DEntity {
    bool CanBePicked(Player player) => true;
    void OnPicked(Player player);
    ICBTexture GetHandTexture();
}
