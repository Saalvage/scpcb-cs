using Veldrid;

namespace scpcb.Graphics.Textures;

public interface IMipmappable {
    void GenerateMipmaps(CommandList commands);
}
