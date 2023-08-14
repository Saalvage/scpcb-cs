using scpcb.Graphics.Primitives;
using ShaderGen;

namespace scpcb.Graphics.Shaders;

public class GeneratedShader<TShader, TVertex, TVertConstants, TFragConstants, TInstanceVertConstants, TInstanceFragConstants>
    : CBShader<TVertex, TVertConstants, TFragConstants, TInstanceVertConstants, TInstanceFragConstants>
    where TVertex : unmanaged
    where TVertConstants : unmanaged
    where TFragConstants : unmanaged
    where TInstanceVertConstants : unmanaged
    where TInstanceFragConstants : unmanaged {
    private const string SHADER_PATH = "Assets/Shaders/";

    public GeneratedShader(GraphicsResources gfxRes)
        : this(gfxRes, gfxRes.GetShaderFileExtension(SHADER_PATH + typeof(TShader).Name, out var spirVRequired), spirVRequired)
    { }

    private GeneratedShader(GraphicsResources gfxRes, string extension, bool spirVRequired) : base(gfxRes,
        File.ReadAllBytes($"{SHADER_PATH}{typeof(TShader).Name}/vertex.{extension}"),
        File.ReadAllBytes($"{SHADER_PATH}{typeof(TShader).Name}/fragment.{extension}"),
        typeof(TShader).GetFields().SingleOrDefault(x => x.FieldType == typeof(TVertConstants))?.Name,
        typeof(TShader).GetFields().SingleOrDefault(x => x.FieldType == typeof(TFragConstants))?.Name,
        typeof(TShader).GetFields().SingleOrDefault(x => x.FieldType == typeof(TInstanceVertConstants))?.Name,
        typeof(TShader).GetFields().SingleOrDefault(x => x.FieldType == typeof(TInstanceFragConstants))?.Name,
        typeof(TShader).GetFields().Where(x => x.FieldType == typeof(Texture2DResource)).Select(x => x.Name).ToArray(),
        typeof(TShader).GetFields().Where(x => x.FieldType == typeof(SamplerResource)).Select(x => x.Name).ToArray(),
            spirVRequired) { }
}
