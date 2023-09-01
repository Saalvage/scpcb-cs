using Veldrid;

namespace scpcb.Graphics; 

public interface IMipmappable {
    void GenerateMipmaps(CommandList commands);
}
