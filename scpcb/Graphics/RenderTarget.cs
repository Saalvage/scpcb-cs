using scpcb.Graphics.Primitives;
using scpcb.Utility;
using Veldrid;

namespace scpcb.Graphics;

public class RenderTarget : Disposable {
    private readonly GraphicsDevice _gfx;

    private readonly CommandList _commands;

    public RenderTarget(GraphicsDevice gfx) {
        _gfx = gfx;
        _commands = gfx.ResourceFactory.CreateCommandList();
    }

    private ICBShader? _lastShader;
    private ICBMaterial? _lastMaterial;
    private ICBMesh? _lastMesh;

    private readonly List<ICBTexture> _generateMipTextures = new();

    public void RegisterForMipmapGeneration(ICBTexture texture) {
        _generateMipTextures.Add(texture);
    }

    public void Start() {
        _commands.Begin();
        _commands.SetFramebuffer(_gfx.SwapchainFramebuffer);
        _commands.ClearColorTarget(0, RgbaFloat.Grey);
        _commands.ClearDepthStencil(1);

        foreach (var t in _generateMipTextures) {
            t.GenerateMipmaps(_commands);
        }
        _generateMipTextures.Clear();
    }

    public void End() {
        _commands.End();
        _gfx.SubmitCommands(_commands);
        _gfx.SwapBuffers();

        _lastShader = null;
        _lastMaterial = null;
        _lastMesh = null;
    }

    public void Render(ICBModel model, float interp) {
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
        
        model.Constants?.UpdateAndSetBuffers(_commands, 1);

        _lastMesh.Draw(_commands);
    }

    protected override void DisposeImpl() {
        _commands.Dispose();
    }
}
