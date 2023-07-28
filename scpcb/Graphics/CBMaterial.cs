using Veldrid;

namespace scpcb.Graphics;

public interface ICBMaterial {
    ICBShader Shader { get; }
    IReadOnlyList<ICBTexture> Textures { get; }

    void Apply(CommandList commands);
}

public interface ICBMaterial<TVertex> : ICBMaterial {
    ICBShader<TVertex> Shader { get; }

    ICBShader ICBMaterial.Shader => Shader;
}

public class CBMaterial<TVertex> : Disposable, ICBMaterial<TVertex> {
    private readonly ResourceSet? _set;
    public ICBShader<TVertex> Shader { get; }

    private readonly ICBTexture[] _textures;
    public IReadOnlyList<ICBTexture> Textures => _textures;

    public CBMaterial(GraphicsDevice gfx, ICBShader<TVertex> shader, ResourceLayout? layout, params ICBTexture[] textures) {
        Shader = shader;
        _textures = textures;
        if (layout != null) {
            _set = gfx.ResourceFactory.CreateResourceSet(new(layout, textures
                .Select(t => (BindableResource)t.View)
                .Append(gfx.Aniso4xSampler)
                .ToArray()));
        }
    }

    public void Apply(CommandList commands) {
        Shader.Apply(commands);
        if (_set != null) {
            commands.SetGraphicsResourceSet(1, _set);
        }
    }

    protected override void DisposeImpl() {
        _set?.Dispose();
    }
}
