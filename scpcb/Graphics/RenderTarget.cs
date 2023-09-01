using scpcb.Graphics.Primitives;
using scpcb.Utility;
using Veldrid;

namespace scpcb.Graphics;

public interface IRenderTarget {
    void Start();
    void End();
    void Render(ICBModel model, float interp);
}

public class RenderTarget : Disposable, IRenderTarget {
    protected readonly GraphicsDevice _gfx;

    protected readonly CommandList _commands;

    public RenderTarget(GraphicsDevice gfx) {
        _gfx = gfx;
        _commands = gfx.ResourceFactory.CreateCommandList();
    }

    private ICBShader? _lastShader;
    private ICBMaterial? _lastMaterial;
    private ICBMesh? _lastMesh;

    protected virtual Framebuffer Framebuffer { get; init; }

    public virtual void Start() {
        _commands.Begin();
        _commands.SetFramebuffer(Framebuffer);
        _commands.ClearColorTarget(0, RgbaFloat.Grey);
        _commands.ClearDepthStencil(1);
    }

    public virtual void End() {
        _commands.End();
        _gfx.SubmitCommands(_commands);

        _lastShader = null;
        _lastMaterial = null;
        _lastMesh = null;
    }

    public void Render(ICBModel model, float interp) {
        if (!model.IsVisible) {
            return;
        }

        if (_lastShader != model.Material.Shader) {
            _lastShader = model.Material.Shader;
            _lastShader.Apply(_commands);
        }
        _lastShader.Constants?.UpdateAndSetBuffers(_commands, 0);

        if (_lastMaterial != model.Material) {
            _lastMaterial = model.Material;
            _lastMaterial.ApplyTextures(_commands);
        }

        if (_lastMesh != model.Mesh) {
            _lastMesh = model.Mesh;
            _lastMesh.ApplyGeometry(_commands);
        }

        // TODO: Revisit this. Is there a better design?
        // At the very least some more caching can be done.
        foreach (var cp in model.ConstantProviders) {
            cp.ApplyTo(_lastShader.Constants!.AsEnumerableElement().Concat(model.Constants.AsEnumerableElementOrEmpty()), interp);
        }
        
        // TODO: Do not hardcode the index! It's not constant!
        model.Constants?.UpdateAndSetBuffers(_commands, 1);

        _lastMesh.Draw(_commands);
    }

    protected override void DisposeImpl() {
        _commands.Dispose();
    }
}
