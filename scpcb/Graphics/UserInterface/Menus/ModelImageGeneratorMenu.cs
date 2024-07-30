using scpcb.Graphics.UserInterface.Composites;
using scpcb.Graphics.UserInterface.Primitives;
using scpcb.Graphics.UserInterface.Utility;
using scpcb.Physics;

namespace scpcb.Graphics.UserInterface.Menus;

public class ModelImageGeneratorMenu : UIElement {
    private readonly ModelImageGenerator _mig;

    public ModelImageGeneratorMenu(GraphicsResources gfxRes, UIManager ui, InputManager input, PhysicsResources physics) {
        PixelSize = new(gfxRes.Window.Width, gfxRes.Window.Height);
        _mig = new(gfxRes, physics, 256, 256);
        _mig.Transform = new();
        _mig.MeshFile = "Assets/Items/gasmask.b3d";

        var img = new TextureElement(gfxRes, _mig.Texture) {
            Alignment = Alignment.Center,
            Position = new(0, -100),
        };
        _internalChildren.Add(img);

        var font = gfxRes.FontCache.GetFont("Assets/Fonts/Courier New.ttf", 32);

        var modelInput = new InputBox(gfxRes, ui, input, font) {
            Alignment = Alignment.Center,
            PixelSize = new(500, 50),
            Position = new(0, 70),
        };
        modelInput.Input.Inner.Text = "Assets/Items/gasmask.b3d";
        modelInput.Input.OnTextChanged += x => {
            _mig.MeshFile = modelInput.Input.Inner.Text;
        };
        _internalChildren.Add(modelInput);

        var button = new Button(gfxRes, ui, "Render", 0f, 0f, 0f) {
            Alignment = Alignment.Center,
            Position = new(0, 200),
            PixelSize = new(300, 80),
        };
        button.OnClicked += () => _mig.Update();
        _internalChildren.Add(button);
    }
}
