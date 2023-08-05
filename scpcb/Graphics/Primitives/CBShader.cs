using scpcb.Graphics.Utility;
using scpcb.Utility;
using Veldrid;
using Veldrid.SPIRV;

namespace scpcb.Graphics.Primitives;

public interface ICBShader : IDisposable {
    ICBMaterial CreateMaterial(IEnumerable<ICBTexture> textures);
    IConstantHolder? TryCreateInstanceConstants();

    IConstantHolder? Constants { get; }
    void Apply(CommandList commands);

    // TODO: Revisit this design?
    int GetTextureSlot();
}

public interface ICBShader<TVertex> : ICBShader {
    new ICBMaterial<TVertex> CreateMaterial(IEnumerable<ICBTexture> textures);
    ICBMaterial ICBShader.CreateMaterial(IEnumerable<ICBTexture> textures) => CreateMaterial(textures);
}

public record struct Empty;

public class CBShader<TVertex, TVertConstants, TFragConstants, TInstanceVertConstants, TInstanceFragConstants>
    : Disposable, ICBShader<TVertex>
        where TVertex : unmanaged
        where TVertConstants : unmanaged where TFragConstants : unmanaged
        where TInstanceVertConstants : unmanaged where TInstanceFragConstants : unmanaged {
    private readonly GraphicsDevice _gfx;

    private readonly Shader[] _shaders;
    private readonly Pipeline _pipeline;

    public ConstantHolder<TVertConstants, TFragConstants>? Constants { get; }
    IConstantHolder? ICBShader.Constants => Constants;

    private readonly ResourceLayout? _constLayout;
    private readonly ResourceLayout? _instanceConstLayout;
    private readonly ResourceLayout? _textureLayout;

    public CBShader(GraphicsDevice gfx, byte[] vertexCode, byte[] fragmentCode, string? vertConstantNames, string? fragConstantNames,
            string? instanceVertConstNames, string? instanceFragConstantNames, IReadOnlyList<string> textureNames, IReadOnlyList<string> samplerNames, bool inputIsSpirV = false) {
        _gfx = gfx;

        _shaders = inputIsSpirV
            // TODO: Veldrid.SPIRV seems to translate away all entrypoint names, so why does it even ask for them??
            ? gfx.ResourceFactory.CreateFromSpirv(new(ShaderStages.Vertex, vertexCode, "main"),
                new ShaderDescription(ShaderStages.Fragment, fragmentCode, "main"))
            : _shaders = new[] {
                gfx.ResourceFactory.CreateShader(new(ShaderStages.Vertex, vertexCode, "main", true)),
                gfx.ResourceFactory.CreateShader(new(ShaderStages.Fragment, fragmentCode, "main", true))
            };

        _constLayout =
            ConstantHolder<TVertConstants, TFragConstants>.TryCreateLayout(gfx, vertConstantNames, fragConstantNames);

        Constants = ConstantHolder<TVertConstants, TFragConstants>.TryCreate(gfx, _constLayout);

        _instanceConstLayout =
            ConstantHolder<TInstanceVertConstants, TInstanceFragConstants>.TryCreateLayout(gfx, instanceVertConstNames,
                instanceFragConstantNames);

        if (textureNames.Count > 0) {
            _textureLayout = gfx.ResourceFactory.CreateResourceLayout(new(textureNames
                .Select(x => new ResourceLayoutElementDescription(x, ResourceKind.TextureReadOnly, ShaderStages.Fragment))
                .Concat(samplerNames
                    .Select(x => new ResourceLayoutElementDescription(x, ResourceKind.Sampler, ShaderStages.Fragment)))
                .ToArray()));
        }

        // TODO: Spir-V bridge does not work for HLSL. (Grab its output and manually try to fix it up to find the issue.)
        _pipeline = gfx.ResourceFactory.CreateGraphicsPipeline(new() {
            BlendState = BlendStateDescription.SingleAlphaBlend,
            DepthStencilState = new(true, true, ComparisonKind.LessEqual),
            RasterizerState = new(FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.CounterClockwise, true, false),
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ResourceLayouts = _constLayout.AsEnumerableElementOrEmpty()
                .Concat(_instanceConstLayout.AsEnumerableElementOrEmpty())
                .Concat(_textureLayout.AsEnumerableElementOrEmpty())
                .ToArray(),
            ShaderSet = new(new[] {
                Helpers.GetDescriptionFromType<TVertex>(),
            }, _shaders),
            Outputs = gfx.SwapchainFramebuffer.OutputDescription,
        });
    }

    public void Apply(CommandList commands) {
        commands.SetPipeline(_pipeline);
    }

    public ICBMaterial<TVertex> CreateMaterial(IEnumerable<ICBTexture> textures)
        => new CBMaterial<TVertex>(_gfx, this, _textureLayout, textures);

    public IConstantHolder? TryCreateInstanceConstants()
        => ConstantHolder<TInstanceVertConstants, TInstanceFragConstants>.TryCreate(_gfx, _instanceConstLayout);

    public int GetTextureSlot()
        => (_constLayout == null ? 0 : 1) + (_instanceConstLayout == null ? 0 : 1);

    protected override void DisposeImpl() {
        Constants?.Dispose();
        _constLayout?.Dispose();
        _instanceConstLayout?.Dispose();
        _textureLayout?.Dispose();

        _pipeline.Dispose();
        foreach (var sh in _shaders) {
            sh.Dispose();
        }
    }
}
