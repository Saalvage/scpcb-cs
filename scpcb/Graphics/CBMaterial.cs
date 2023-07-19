using Veldrid;

namespace scpcb;

public interface ICBMaterial {
    void Apply(CommandList commands);
    Type GetVertexType();
}

public interface ICBMaterial<TVertex> : ICBMaterial {
    Type ICBMaterial.GetVertexType() => typeof(TVertex);
}

public class CBMaterial<TVertex> : Disposable, ICBMaterial<TVertex> {
    private readonly ResourceSet? _set;
    private readonly ICBShader<TVertex> _shader;

    private readonly ICBTexture[] _textures; // Keep alive TODO: Needed? Garbage collection was a mistake!

    public CBMaterial(GraphicsDevice gfx, ICBShader<TVertex> shader, ResourceLayout? layout, params ICBTexture[] textures) {
        _shader = shader;
        _textures = textures;
        if (layout != null) {
            _set = gfx.ResourceFactory.CreateResourceSet(new(layout, textures
                .Select(t => (BindableResource)t.View)
                .Append(gfx.Aniso4xSampler)
                .ToArray()));
        }
    }

    public void Apply(CommandList commands) {
        _shader.Apply(commands);
        if (_set != null) {
            commands.SetGraphicsResourceSet(1, _set);
        }
    }

    protected override void DisposeImpl() {
        _set?.Dispose();
    }
}
