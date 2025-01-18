using System.Diagnostics;
using SCPCB.Graphics.Shaders.Utility;
using SCPCB.Utility;
using Veldrid;
using Veldrid.SPIRV;

namespace SCPCB.Graphics.Primitives;

public interface ICBShader : IDisposable {
    ICBMaterial CreateMaterial(IEnumerable<ICBTexture> textures, IEnumerable<Sampler> samplers);

    IConstantHolder? TryCreateInstanceConstants();

    IConstantHolder? Constants { get; }
    void Apply(CommandList commands);

    int TextureCount { get; }
    int SamplerCount { get; } 

    uint ConstantSlot { get; }
    uint InstanceConstantSlot { get; }
    uint TextureSlot { get; }
}

public interface ICBShader<TVertex> : ICBShader {
    new ICBMaterial<TVertex> CreateMaterial(IEnumerable<ICBTexture> textures, IEnumerable<Sampler> samplers);
    ICBMaterial ICBShader.CreateMaterial(IEnumerable<ICBTexture> textures, IEnumerable<Sampler> samplers) => CreateMaterial(textures, samplers);
}

public record struct Empty;

public class CBShader<TVertex, TVertConstants, TFragConstants, TInstanceVertConstants, TInstanceFragConstants>
    : Disposable, ICBShader<TVertex>
        where TVertex : unmanaged
        where TVertConstants : unmanaged where TFragConstants : unmanaged
        where TInstanceVertConstants : unmanaged where TInstanceFragConstants : unmanaged {

    private class CBMaterial : Disposable, ICBMaterial<TVertex> {
        private readonly GraphicsDevice _gfx;
        private readonly ResourceLayout _layout;

        private ResourceSet? _set;
        public ICBShader<TVertex> Shader { get; }

        private readonly bool _isStatic;
        private long _lastTextureHash;
        private readonly Dictionary<long, ResourceSet> _sets;

        private readonly Sampler[] _samplers;
        private readonly ICBTexture[] _textures;
        public IReadOnlyList<ICBTexture> Textures => _textures;

        public CBMaterial(GraphicsDevice gfx, ICBShader<TVertex> shader, ResourceLayout? layout,
                IEnumerable<ICBTexture> textures, IEnumerable<Sampler> samplers) {
            _gfx = gfx;
            _layout = layout;

            _samplers = samplers.ToArray();

            Shader = shader;

            // Defensive copy.
            _textures = textures.ToArray();
            _lastTextureHash = GetTexturesHashCode();
            if (layout != null) {
                _set = CreateSet();

                _isStatic = _textures.All(x => x.IsStatic);
                if (!_isStatic) {
                    _sets = new() {
                        [GetTexturesHashCode()] = _set,
                    };
                }
            }
        }

        public void ApplyTextures(CommandList commands) {
            if (_set != null) {
                if (!_isStatic) {
                    var newHash = GetTexturesHashCode();
                    if (newHash != _lastTextureHash) {
                        _set = _sets.TryGetValue(newHash, out var set)
                            ? set
                            : _sets[newHash] = CreateSet();
                        _lastTextureHash = newHash;
                    }
                }
                commands.SetGraphicsResourceSet(Shader.TextureSlot, _set);
            }
        }

        private long GetTexturesHashCode()
            => Textures.Select(x => x.GetHashCode()).Aggregate(0, HashCode.Combine);

        private ResourceSet CreateSet()
            => _gfx.ResourceFactory.CreateResourceSet(new(_layout, _textures
                .Select(BindableResource (t) => t.View)
                .Concat(_samplers)
                .ToArray()));

        protected override void DisposeImpl() {
            _set?.Dispose();
        }
    }

    private readonly GraphicsDevice _gfx;

    private readonly Shader[] _shaders;
    private readonly Pipeline _pipeline;

    public ConstantHolder<TVertConstants, TFragConstants>? Constants { get; }
    IConstantHolder? ICBShader.Constants => Constants;

    private readonly ResourceLayout? _constLayout;
    private readonly ResourceLayout? _instanceConstLayout;
    private readonly ResourceLayout? _textureLayout;

    public CBShader(GraphicsResources gfxRes, byte[] vertexCode, byte[] fragmentCode, string vertexEntryPoint, string fragmentEntryPoint,
            string? vertConstantNames, string? fragConstantNames, string? instanceVertConstNames, string? instanceFragConstantNames,
            IReadOnlyList<string> textureNames, IReadOnlyList<string> samplerNames,
            ShaderParameters? shaderParameterOverrides = null, bool inputIsSpirV = false) {
        _gfx = gfxRes.GraphicsDevice;

        _shaders = inputIsSpirV
            // TODO: Veldrid.SPIRV seems to translate away all entrypoint names, so why does it even ask for them??
            ? _gfx.ResourceFactory.CreateFromSpirv(new(ShaderStages.Vertex, vertexCode, "main"),
                new ShaderDescription(ShaderStages.Fragment, fragmentCode, "main"))
            : [
                _gfx.ResourceFactory.CreateShader(new(ShaderStages.Vertex, vertexCode, vertexEntryPoint, gfxRes.Debug)),
                _gfx.ResourceFactory.CreateShader(new(ShaderStages.Fragment, fragmentCode, fragmentEntryPoint, gfxRes.Debug)),
            ];

        _constLayout =
            ConstantHolder<TVertConstants, TFragConstants>.TryCreateLayout(_gfx, vertConstantNames, fragConstantNames);

        Constants = ConstantHolder<TVertConstants, TFragConstants>.TryCreate(_gfx, _constLayout);

        _instanceConstLayout =
            ConstantHolder<TInstanceVertConstants, TInstanceFragConstants>.TryCreateLayout(_gfx, instanceVertConstNames,
                instanceFragConstantNames);

        SamplerCount = samplerNames.Count;
        TextureCount = textureNames.Count;
        if (textureNames.Count > 0) {
            _textureLayout = _gfx.ResourceFactory.CreateResourceLayout(new(textureNames
                .Select(x => new ResourceLayoutElementDescription(x, ResourceKind.TextureReadOnly, ShaderStages.Fragment))
                .Concat(samplerNames
                    .Select(x => new ResourceLayoutElementDescription(x, ResourceKind.Sampler, ShaderStages.Fragment)))
                .ToArray()));
        }

        ConstantSlot = 0u;
        InstanceConstantSlot = ConstantSlot + (_instanceConstLayout == null ? 0u : 1u);
        TextureSlot = InstanceConstantSlot + (_textureLayout == null ? 0u : 1u);

        // ReSharper disable once VirtualMemberCallInConstructor
        var shaderParameters = shaderParameterOverrides ?? GetDefaultParameters();

        // TODO: Spir-V bridge does not work for HLSL. (Grab its output and manually try to fix it up to find the issue.)
        _pipeline = _gfx.ResourceFactory.CreateGraphicsPipeline(new() {
            BlendState = shaderParameters.BlendState,
            DepthStencilState = shaderParameters.DepthState,
            RasterizerState = shaderParameters.RasterizerState,
            PrimitiveTopology = shaderParameters.Topology,
            ResourceLayouts = _constLayout.AsEnumerableElementOrEmpty()
                .Concat(_instanceConstLayout.AsEnumerableElementOrEmpty())
                .Concat(_textureLayout.AsEnumerableElementOrEmpty())
                .ToArray(),
            ShaderSet = new([
                Helpers.GetDescriptionFromType<TVertex>(),
            ], _shaders),
            Outputs = _gfx.SwapchainFramebuffer.OutputDescription,
        });
    }

    public void Apply(CommandList commands) {
        commands.SetPipeline(_pipeline);
    }

    public ICBMaterial<TVertex> CreateMaterial(IEnumerable<ICBTexture> textures, IEnumerable<Sampler> samplers) {
        Debug.Assert(textures.Count() == TextureCount);
        Debug.Assert(samplers.Count() == SamplerCount);
        return new CBMaterial(_gfx, this, _textureLayout, textures, samplers);
    }

    public IConstantHolder? TryCreateInstanceConstants()
        => ConstantHolder<TInstanceVertConstants, TInstanceFragConstants>.TryCreate(_gfx, _instanceConstLayout);

    public int TextureCount { get; }
    public int SamplerCount { get; }

    public uint ConstantSlot { get; }
    public uint InstanceConstantSlot { get; }
    public uint TextureSlot { get; }
    
    /// <summary>
    /// Mustn't depend on instance variables.
    /// </summary>
    protected virtual ShaderParameters GetDefaultParameters() => ShaderParameters.Default;

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
