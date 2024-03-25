using scpcb.Entities;
using scpcb.Graphics.UserInterface;

namespace scpcb.PlayerController;

public partial class Player {
    private IPickableEntity? _closestPickable;

    private void UpdatePickables() {
        // Inefficient, but likely negligible impact on performance.
        var newClosestPickable = _scene.GetEntitiesOfType<IPickableEntity>()
            .Where(x => x.CanBePicked(this) && (x.Position - Camera.Position).LengthSquared() < 4)
            .MinBy(x => (Camera.Position - x.Position).LengthSquared());

        if (newClosestPickable != _closestPickable) {
            var ui = _scene.GetEntitiesOfType<UIManager>().Single();
            var children = ui.Root.Children;
            if (_closestPickable != null) {
                children.Remove(_currHand);
            }

            if (newClosestPickable != null) {
                _currHand = new TextureElement(ui.GraphicsResources, newClosestPickable.GetHandTexture()) { Alignment = Alignment.Center };
                ui.Root.Children.Add(_currHand);
            }
        }

        _closestPickable = newClosestPickable;
    }
}
