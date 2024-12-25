using SCPCB.Graphics.Primitives;
using Veldrid;

namespace SCPCB.Graphics.Textures;

public interface IMipmappable : ICBTexture {
    void GenerateMipmaps(CommandList commands);
}
