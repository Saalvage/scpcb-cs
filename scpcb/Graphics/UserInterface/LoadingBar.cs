using System.Drawing;
using scpcb.Graphics.Primitives;
using scpcb.Utility;

namespace scpcb.Graphics.UserInterface;

public class LoadingBar : Border {
    public LoadingBar(GraphicsResources gfxRes, int count, ICBTexture texture)
        : base(gfxRes, new(10 * count + 4, 20), 1, Color.White) {
        Children.AddRange(Enumerable.Range(0, count)
            .Select(i => new TextureElement(gfxRes, texture) {
                Position = new(3 + 10 * i, 3),
            }));
    }
}
