using scpcb.Graphics.Primitives;
using Veldrid;

namespace scpcb.Graphics.Textures;

public interface IMipmappable : ICBTexture {
    void GenerateMipmaps(CommandList commands);
}
