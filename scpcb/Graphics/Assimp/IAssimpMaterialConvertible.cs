using Assimp;
using SCPCB.Graphics.Primitives;

namespace SCPCB.Graphics.Assimp; 

// TODO: Make this generic instead of Assimp specific.
// In general, I don't think this should be attached to shaders. Just always make the mat converter a plugin?
public interface IAssimpMaterialConvertible<TVertex, TPlugin> {
    public static abstract ICBMaterial<TVertex> ConvertMaterial(Material mat, string fileDir, TPlugin plugin);
}
