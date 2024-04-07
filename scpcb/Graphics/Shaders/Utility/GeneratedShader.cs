using System.Reflection;
using scpcb.Graphics.Primitives;
using scpcb.Utility;
using ShaderGen;

namespace scpcb.Graphics.Shaders.Utility;

public class GeneratedShader<TShader, TVertex, TVertConstants, TFragConstants, TInstanceVertConstants, TInstanceFragConstants>
    : CBShader<TVertex, TVertConstants, TFragConstants, TInstanceVertConstants, TInstanceFragConstants>
    where TShader : IAutoShader<TVertConstants, TFragConstants, TInstanceVertConstants, TInstanceFragConstants>
    where TVertex : unmanaged
    where TVertConstants : unmanaged
    where TFragConstants : unmanaged
    where TInstanceVertConstants : unmanaged
    where TInstanceFragConstants : unmanaged {
    private const string SHADER_PATH = "Assets/Shaders/";

    public GeneratedShader(GraphicsResources gfxRes, ShaderParameters? shaderParameterOverrides = null)
        : this(gfxRes, shaderParameterOverrides, gfxRes.GetShaderFileExtension(SHADER_PATH + typeof(TShader).Name, out var spirVRequired),
            GetMethodWithSingleParameter(typeof(TVertex)),
            spirVRequired)
    { }

    private GeneratedShader(GraphicsResources gfxRes, ShaderParameters? shaderParameterOverrides,
            string extension, MethodInfo vs, bool spirVRequired) : base(gfxRes,
        File.ReadAllBytes($"{SHADER_PATH}{typeof(TShader).Name}/vertex.{extension}"),
        File.ReadAllBytes($"{SHADER_PATH}{typeof(TShader).Name}/fragment.{extension}"),
        vs.Name,
        GetMethodWithSingleParameter(vs.ReturnType).Name,
        GetBlockName<TVertConstants>(),
        GetBlockName<TFragConstants>(),
        GetBlockName<TInstanceVertConstants>(),
        GetBlockName<TInstanceFragConstants>(),
        GetMembersOfType<Texture2DResource>().ToArray(),
        GetMembersOfType<SamplerResource>().ToArray(),
        shaderParameterOverrides,
        spirVRequired) {
        Log.Information("Loading shader {type}", typeof(TShader));
    }

    protected override ShaderParameters GetDefaultParameters() => TShader.DefaultParameters;

    private static MethodInfo GetMethodWithSingleParameter(Type paramType)
        => typeof(TShader).GetMethods()
            .Single(x => x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == paramType);

    private static string? GetBlockName<TMember>()
        => typeof(TMember) == typeof(Empty) ? null : GetMembersOfType<TMember>().SingleOrDefault();

    private static IEnumerable<string> GetMembersOfType<TMember>()
        => typeof(TShader)
            .GetFieldsAndProperties()
            .Where(x => x.Type == typeof(TMember))
            .Select(x => x.Member.Name);
}
