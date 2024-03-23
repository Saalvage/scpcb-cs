using scpcb.Graphics.Primitives;

namespace scpcb.Entities;

public interface IPickableEntity : I3DEntity {
    bool CanBePicked(IPlayer player) => true;
    void OnPicked(IPlayer player);
    ICBTexture GetHandTexture();
}
