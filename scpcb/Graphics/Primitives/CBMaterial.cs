using scpcb.Utility;
using Veldrid;

namespace scpcb.Graphics.Primitives;

public interface ICBMaterial : IDisposable {
    ICBShader Shader { get; }
    IReadOnlyList<ICBTexture> Textures { get; }

    void ApplyTextures(CommandList commands);
}

public interface ICBMaterial<TVertex> : ICBMaterial {
    new ICBShader<TVertex> Shader { get; }
    ICBShader ICBMaterial.Shader => Shader;
}

public class CBMaterial<TVertex> : Disposable, ICBMaterial<TVertex> {
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

    /// <summary>
    /// Do not call this directly! Use <see cref="ICBShader.CreateMaterial"/> instead.
    /// </summary>
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
            commands.SetGraphicsResourceSet((uint)Shader.GetTextureSlot(), _set);
        }
    }

    private long GetTexturesHashCode()
        => Textures.Count switch {
            0 => 0,
            1 => Textures[0].View.GetHashCode(),
            2 => HashCode.Combine(Textures[0].View.GetHashCode(), Textures[1].View.GetHashCode()),
            3 => HashCode.Combine(Textures[0].View.GetHashCode(), Textures[1].View.GetHashCode(), Textures[2].View.GetHashCode()),
            4 => HashCode.Combine(Textures[0].View.GetHashCode(), Textures[1].View.GetHashCode(), Textures[2].View.GetHashCode(), Textures[3].View.GetHashCode()),
            5 => HashCode.Combine(Textures[0].View.GetHashCode(), Textures[1].View.GetHashCode(), Textures[2].View.GetHashCode(), Textures[3].View.GetHashCode(), Textures[4].View.GetHashCode()),
            6 => HashCode.Combine(Textures[0].View.GetHashCode(), Textures[1].View.GetHashCode(), Textures[2].View.GetHashCode(), Textures[3].View.GetHashCode(), Textures[4].View.GetHashCode(), Textures[5].View.GetHashCode()),
            7 => HashCode.Combine(Textures[0].View.GetHashCode(), Textures[1].View.GetHashCode(), Textures[2].View.GetHashCode(), Textures[3].View.GetHashCode(), Textures[4].View.GetHashCode(), Textures[5].View.GetHashCode(), Textures[6].View.GetHashCode()),
            8 => HashCode.Combine(Textures[0].View.GetHashCode(), Textures[1].View.GetHashCode(), Textures[2].View.GetHashCode(), Textures[3].View.GetHashCode(), Textures[4].View.GetHashCode(), Textures[5].View.GetHashCode(), Textures[6].View.GetHashCode(), Textures[7].View.GetHashCode()),
        };

    private ResourceSet CreateSet()
        => _gfx.ResourceFactory.CreateResourceSet(new(_layout, _textures
            .Select(t => (BindableResource)t.View)
            .Concat(_samplers)
            .ToArray()));

    protected override void DisposeImpl() {
        _set?.Dispose();
    }
}
