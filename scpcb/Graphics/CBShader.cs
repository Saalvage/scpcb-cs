using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using scpcb.Graphics.Shaders.ConstantMembers;
using Veldrid;
using Veldrid.SPIRV;

namespace scpcb.Graphics;

public interface ICBShader {
    ICBMaterial CreateMaterial(ICBTexture[] textures);
    void Apply(CommandList commands);

    bool HasVertexConstant<T>() where T : IConstantMember;
    bool HasFragmentConstant<T>() where T : IConstantMember;
    void SetVertexConstantValue<T, TVal>(TVal val) where T : IConstantMember<T, TVal> where TVal : unmanaged;
    void SetFragmentConstantValue<T, TV>(TV val) where T : IConstantMember<T, TV> where TV : unmanaged;
}

public interface ICBShader<TVertex> : ICBShader {
    ICBMaterial ICBShader.CreateMaterial(ICBTexture[] textures) => CreateMaterial(textures);
    new ICBMaterial<TVertex> CreateMaterial(ICBTexture[] textures);
}

public record struct Empty;

public class CBShader<TVertex, TVertConstants, TFragConstants> : Disposable, ICBShader<TVertex>
        where TVertex : unmanaged
        where TVertConstants : unmanaged
        where TFragConstants : unmanaged {
    private readonly GraphicsDevice _gfx;

    private readonly Shader[] _shaders;
    private readonly Pipeline _pipeline;

    private readonly DeviceBuffer? _vertexConstantBuffer;
    private readonly DeviceBuffer? _fragmentConstantBuffer;
    private readonly ResourceLayout? _constLayout;
    private readonly ResourceLayout? _textureLayout;
    private readonly ResourceSet? _set;

    private TVertConstants _lastVertConstants;
    private readonly object _vertexConstantsBoxed = default(TVertConstants);
    public ref TVertConstants VertexConstants => ref Unsafe.Unbox<TVertConstants>(_vertexConstantsBoxed);

    private TFragConstants _lastFragConstants;
    private readonly object _fragmentConstantsBoxed = default(TFragConstants);
    public ref TFragConstants FragmentConstants => ref Unsafe.Unbox<TFragConstants>(_fragmentConstantsBoxed);

    private readonly int _textureCount;

    public CBShader(GraphicsDevice gfx, string vertexFile, string fragmentFile, int textureCount)
        : this(gfx, File.ReadAllBytes(vertexFile), File.ReadAllBytes(fragmentFile), textureCount) { }

    public unsafe CBShader(GraphicsDevice gfx, byte[] vertexCode, byte[] fragmentCode, int textureCount, bool inputIsSpirV = false) {
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
                // TODO: Don't hardcode names, get them as a parameter?
                layouts.Add(new("VConstants", ResourceKind.UniformBuffer, ShaderStages.Vertex));
            }

            if (hasFragConsts) {
                _fragmentConstantBuffer = CreateBuffer<TFragConstants>();
                consts.Add(_fragmentConstantBuffer);
                layouts.Add(new("FConstants", ResourceKind.UniformBuffer, ShaderStages.Fragment));
            }

            _constLayout = gfx.ResourceFactory.CreateResourceLayout(new(layouts.ToArray()));
            if (textureCount > 0) {
                _textureLayout = gfx.ResourceFactory.CreateResourceLayout(new(Enumerable.Range(0, textureCount)
                    .Select(x => new ResourceLayoutElementDescription($"texture{x}", ResourceKind.TextureReadOnly, ShaderStages.Fragment))
                    .Append(new("samper", ResourceKind.Sampler, ShaderStages.Fragment))
                    .ToArray()));
                // TODO: Do not hardcode these names, although it seems like it currently does not break things!
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

        _shaders = inputIsSpirV
            ? gfx.ResourceFactory.CreateFromSpirv(new(ShaderStages.Vertex, vertexCode, "main"),
                new ShaderDescription(ShaderStages.Fragment, fragmentCode, "main"))
            : _shaders = new[] {
                gfx.ResourceFactory.CreateShader(new(ShaderStages.Vertex, vertexCode,
                    gfx.BackendType == GraphicsBackend.Vulkan ? "main" : "VS", true)), // TODO: Why is this now??
                gfx.ResourceFactory.CreateShader(new(ShaderStages.Fragment, fragmentCode,
                    gfx.BackendType == GraphicsBackend.Vulkan ? "main" : "FS", true)),
            };

        _pipeline = gfx.ResourceFactory.CreateGraphicsPipeline(new() {
            BlendState = BlendStateDescription.SingleAlphaBlend,
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
            UpdateBuffer(commands, _vertexConstantBuffer, ref VertexConstants, ref _lastVertConstants);
            UpdateBuffer(commands, _fragmentConstantBuffer, ref FragmentConstants, ref _lastFragConstants);

            // This is a pretty hot path (on avg. 1 invocation per shader per frame) so further optimizations might be warranted.
            // See Span's SequenceEqual.
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            static unsafe void UpdateBuffer<T>(CommandList commands, DeviceBuffer? buffer, ref T curr, ref T prev) where T : unmanaged {
                if (buffer == null) {
                    return;
                }

                fixed (T* currPtr = &curr) {
                    fixed (T* prevPtr = &prev) {
                        var currBytes = (byte*)currPtr;
                        var prevBytes = (byte*)prevPtr;

                        var offset = 0;
                        for (; offset < sizeof(T); offset++) {
                            if (currBytes[offset] != prevBytes[offset]) {
                                break;
                            }
                        }

                        var differentUntil = sizeof(T) - 1;
                        for (; differentUntil > offset; differentUntil--) {
                            if (currBytes[differentUntil] == prevBytes[differentUntil]) {
                                break;
                            }
                        }

                        if (offset != sizeof(T)) {
                            var dataToUpdate = new Span<byte>(currBytes + offset, differentUntil - offset + 1);
                            commands.UpdateBuffer(buffer, (uint)offset, dataToUpdate);
                            prev = curr;
                        }
                    }
                }
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

    public bool HasVertexConstant<T>() where T : IConstantMember
        => _vertexConstantsBoxed is T;

    public bool HasFragmentConstant<T>() where T : IConstantMember
        => _fragmentConstantsBoxed is T;

    public void SetVertexConstantValue<T, TVal>(TVal val) where T : IConstantMember<T, TVal> where TVal : unmanaged {
        if (_vertexConstantsBoxed is T t) {
            t.Value = val;
        }
    }

    public void SetFragmentConstantValue<T, TVal>(TVal val) where T : IConstantMember<T, TVal> where TVal : unmanaged {
        if (_fragmentConstantsBoxed is T t) {
            t.Value = val;
        }
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
