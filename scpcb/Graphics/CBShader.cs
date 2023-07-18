using System.Runtime.InteropServices;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace scpcb;

public interface ICBShader {
    ICBMaterial CreateMaterial(ICBTexture[] textures);
    void Apply(CommandList commands);
}

public interface ICBShader<TVertex> : ICBShader {
    ICBMaterial ICBShader.CreateMaterial(ICBTexture[] textures) => CreateMaterial(textures);
    new ICBMaterial<TVertex> CreateMaterial(ICBTexture[] textures);
}

public record struct Empty;

public class CBShader<TVertex, TVertConstants, TFragConstants> : Disposable, ICBShader<TVertex>
    where TVertex : unmanaged
    where TVertConstants : unmanaged, IEquatable<TVertConstants>
    where TFragConstants : unmanaged, IEquatable<TFragConstants> {
    private readonly GraphicsDevice _gfx;

    private readonly Shader[] _shaders;
    private readonly Pipeline _pipeline;

    private readonly DeviceBuffer? _vertexConstantBuffer;
    private readonly DeviceBuffer? _fragmentConstantBuffer;
    private readonly ResourceLayout? _constLayout;
    private readonly ResourceLayout? _textureLayout;
    private readonly ResourceSet? _set;

    private TVertConstants _lastVertConstants;
    public TVertConstants VertexConstants;

    private TFragConstants _lastFragConstants;
    public TFragConstants FragmentConstants;

    private readonly int _textureCount;

    public unsafe CBShader(GraphicsDevice gfx, string vertexCode, string fragmentCode, int textureCount) {
        _gfx = gfx;
        _textureCount = textureCount;
        var hasVertConsts = typeof(TVertConstants) != typeof(Empty);
        var hasFragConsts = typeof(TFragConstants) != typeof(Empty);
        if (hasVertConsts || hasFragConsts) {
            var consts = new List<BindableResource>();
            var layouts = new List<ResourceLayoutElementDescription>();
            if (hasVertConsts) {
                _vertexConstantBuffer = CreateBuffer<TVertConstants>();
                consts.Add(_vertexConstantBuffer);
                // TODO: Don't hardcode names, get them from reflection?
                layouts.Add(new("constants", ResourceKind.UniformBuffer, ShaderStages.Vertex));
            }

            if (hasFragConsts) {
                _fragmentConstantBuffer = CreateBuffer<TFragConstants>();
                consts.Add(_fragmentConstantBuffer);
                layouts.Add(new("constants", ResourceKind.UniformBuffer, ShaderStages.Fragment));
            }

            _constLayout = gfx.ResourceFactory.CreateResourceLayout(new(layouts.ToArray()));
            if (textureCount > 0) {
                _textureLayout = gfx.ResourceFactory.CreateResourceLayout(new(Enumerable.Range(0, textureCount)
                    .Select(x => new ResourceLayoutElementDescription($"texture{x}", ResourceKind.TextureReadOnly, ShaderStages.Fragment))
                    .Append(new("samper", ResourceKind.Sampler, ShaderStages.Fragment))
                    .ToArray()));
            }
            _set = gfx.ResourceFactory.CreateResourceSet(new(_constLayout, consts.ToArray()));

            DeviceBuffer CreateBuffer<T>() where T : unmanaged {
                var propertySize = typeof(T).GetProperties().Sum(x => Marshal.SizeOf(x.PropertyType));
                if (propertySize != 0 && propertySize != sizeof(T)) { // TODO: Deal with 0
                    throw new InvalidOperationException("Size of struct does not equal sum of properties");
                }
                return gfx.ResourceFactory.CreateBuffer(new(RoundTo32Bytes(propertySize), BufferUsage.UniformBuffer));

                static uint RoundTo32Bytes(int num) {
                    uint run = 32;
                    while (run < num) {
                        run += 32;
                    }
                    return run;
                }
            }
        }

        _shaders = gfx.ResourceFactory.CreateFromSpirv(new(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vertexCode), "main"),
            new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(fragmentCode), "main"));
        _pipeline = gfx.ResourceFactory.CreateGraphicsPipeline(new() {
            BlendState = BlendStateDescription.SingleOverrideBlend,
            DepthStencilState = new(true, true, ComparisonKind.LessEqual),
            RasterizerState = new(FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.CounterClockwise, true, false),
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ResourceLayouts = Extensions.FromSingleOrEmpty(_constLayout).Concat(Extensions.FromSingleOrEmpty(_textureLayout)).ToArray(),
            ShaderSet = new(new[] {
                Helpers.GetDescriptionFromType<TVertex>(),
            }, _shaders),
            Outputs = gfx.SwapchainFramebuffer.OutputDescription,
        });
    }

    public void Apply(CommandList commands) {
        // TODO: OPT Only set pipeline when necessary?
        commands.SetPipeline(_pipeline);
        if (_set != null) {
            if (!VertexConstants.Equals(_lastVertConstants)) {
                _lastVertConstants = VertexConstants;
                // TODO: OPT Only update necessary data?
                commands.UpdateBuffer(_vertexConstantBuffer, 0, ref VertexConstants);
            }

            if (!FragmentConstants.Equals(_lastFragConstants)) {
                _lastFragConstants = FragmentConstants;
                commands.UpdateBuffer(_fragmentConstantBuffer, 0, ref FragmentConstants);
            }

            commands.SetGraphicsResourceSet(0, _set);
        }
    }

    public ICBMaterial<TVertex> CreateMaterial(params ICBTexture[] textures) {
        if (textures.Length != _textureCount) {
            throw new ArgumentException("Incorrect number of textures");
        }
        return new CBMaterial<TVertex>(_gfx, this, _textureLayout, textures);
    }

    protected override void DisposeImpl() {
        _pipeline.Dispose();
        foreach (var sh in _shaders) {
            sh.Dispose();
        }
        _vertexConstantBuffer?.Dispose();
        _fragmentConstantBuffer?.Dispose();
        _constLayout?.Dispose();
        _textureLayout?.Dispose();
        _set?.Dispose();
    }
}
