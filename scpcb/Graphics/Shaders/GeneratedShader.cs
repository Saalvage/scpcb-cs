using System.Reflection;
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
        : this(gfxRes, gfxRes.GetShaderFileExtension(SHADER_PATH + typeof(TShader).Name, out var spirVRequired),
            GetMethodWithSingleParameter(typeof(TVertex)),
            spirVRequired)
    { }

    private GeneratedShader(GraphicsResources gfxRes, string extension, MethodInfo vs, bool spirVRequired) : base(gfxRes,
        File.ReadAllBytes($"{SHADER_PATH}{typeof(TShader).Name}/vertex.{extension}"),
        File.ReadAllBytes($"{SHADER_PATH}{typeof(TShader).Name}/fragment.{extension}"),
        vs.Name,
        GetMethodWithSingleParameter(vs.ReturnType).Name,
        GetFieldsOfType<TVertConstants>().SingleOrDefault(),
        GetFieldsOfType<TFragConstants>().SingleOrDefault(),
        GetFieldsOfType<TInstanceVertConstants>().SingleOrDefault(),
        GetFieldsOfType<TInstanceFragConstants>().SingleOrDefault(),
        GetFieldsOfType<Texture2DResource>().ToArray(),
        GetFieldsOfType<SamplerResource>().ToArray(),
        spirVRequired) { }

    private static MethodInfo GetMethodWithSingleParameter(Type paramType)
        => typeof(TShader).GetMethods()
            .Single(x => x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == paramType);

    private static IEnumerable<string> GetFieldsOfType<TField>()
        => typeof(TShader)
            .GetFields()
            .Where(x => x.FieldType == typeof(TField))
            .Select(x => x.Name);
}
