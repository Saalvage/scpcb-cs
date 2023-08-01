using ShaderGen;
using System.Reflection;

namespace scpcb.Graphics.Shaders;

public class GeneratedShader<TShader, TVertex, TVertConstants, TFragConstants> : CBShader<TVertex, TVertConstants, TFragConstants>
    where TVertex : unmanaged
    where TVertConstants : unmanaged
    where TFragConstants : unmanaged {
    private const string SHADER_PATH = "Assets/Shaders/";

    public GeneratedShader(GraphicsResources gfxRes)
        : this(gfxRes, gfxRes.GetShaderFileExtension(SHADER_PATH + typeof(TShader).Name, out var spirVRequired), spirVRequired)
    { }

    private GeneratedShader(GraphicsResources gfxRes, string extension, bool spirVRequired) : base(gfxRes.GraphicsDevice,
        File.ReadAllBytes($"{SHADER_PATH}{typeof(TShader).Name}/vertex.{extension}"),
        File.ReadAllBytes($"{SHADER_PATH}{typeof(TShader).Name}/fragment.{extension}"),
        typeof(TShader).GetFields().SingleOrDefault(x => x.FieldType == typeof(TVertConstants))?.Name,
        typeof(TShader).GetFields().SingleOrDefault(x => x.FieldType == typeof(TFragConstants))?.Name,
        typeof(TShader).GetFields().Where(x => x.FieldType == typeof(Texture2DResource)).Select(x => x.Name).ToArray(),
        typeof(TShader).GetFields().Where(x => x.FieldType == typeof(SamplerResource)).Select(x => x.Name).ToArray(),
            spirVRequired) { }
}
