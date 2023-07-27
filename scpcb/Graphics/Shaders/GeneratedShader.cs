using ShaderGen;
using System.Reflection;

namespace scpcb.Graphics.Shaders;

public class GeneratedShader<TShader, TVertex, TVertConstants, TFragConstants> : CBShader<TVertex, TVertConstants, TFragConstants>
    where TVertex : unmanaged
    where TVertConstants : unmanaged
    where TFragConstants : unmanaged {
    private const string SHADER_PATH_FORMAT = "Assets/Shaders/{0}-{1}.{2}";

    public GeneratedShader(GraphicsResources gfxRes) : base(gfxRes.GraphicsDevice,
        string.Format(SHADER_PATH_FORMAT, typeof(TShader).Name, "vertex", gfxRes.ShaderFileExtension),
        string.Format(SHADER_PATH_FORMAT, typeof(TShader).Name, "fragment", gfxRes.ShaderFileExtension),
        typeof(TShader).GetRuntimeFields().Count(x => x.FieldType == typeof(Texture2DResource))) { }
}
