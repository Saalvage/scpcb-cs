using System.Drawing;
using System.Numerics;

namespace scpcb.Graphics.UserInterface;

public class DebugBorder : Border {
    public DebugBorder(GraphicsResources gfxRes, Vector2 dimensions, float thickness, Color color) : base(gfxRes, dimensions, thickness, color) { }
}
