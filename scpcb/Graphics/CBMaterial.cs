using Veldrid;

namespace scpcb.Graphics;

public interface ICBMaterial {
    ICBShader Shader { get; }
    IReadOnlyList<ICBTexture> Textures { get; }

    void ApplyTextures(CommandList commands);
}

public interface ICBMaterial<TVertex> : ICBMaterial {
    new ICBShader<TVertex> Shader { get; }
    ICBShader ICBMaterial.Shader => Shader;
}

public class CBMaterial<TVertex> : Disposable, ICBMaterial<TVertex> {
    private readonly ResourceSet? _set;
    public ICBShader<TVertex> Shader { get; }

    private readonly ICBTexture[] _textures;
    public IReadOnlyList<ICBTexture> Textures => _textures;

    public CBMaterial(GraphicsDevice gfx, ICBShader<TVertex> shader, ResourceLayout? layout, IEnumerable<ICBTexture> textures) {
        Shader = shader;
        // Defensive copy.
        _textures = textures.ToArray();
        if (layout != null) {
            _set = gfx.ResourceFactory.CreateResourceSet(new(layout, _textures
                .Select(t => (BindableResource)t.View)
                .Append(gfx.Aniso4xSampler)
                .ToArray()));
        }
    }

    public void ApplyTextures(CommandList commands) {
        if (_set != null) {
            commands.SetGraphicsResourceSet((uint)Shader.GetTextureSlot(), _set);
        }
    }

    protected override void DisposeImpl() {
        _set?.Dispose();
    }
}
