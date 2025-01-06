using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders.Utility;
using SCPCB.Utility;
using Veldrid;

namespace SCPCB.Graphics.Textures;

public interface IRenderTarget {
    void Start();
    void End();
    // Using generics here to avoid potential GC pressure due to the boxed structs.
    void Render<TVertex>(MeshMaterial<TVertex> model, IConstantHolder? instanceHolder = null) where TVertex : unmanaged;
    void ClearDepthStencil();
    public void PushScissor(uint x, uint y, uint w, uint h);
    public void PopScissor();
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

    private readonly Stack<(uint X, uint Y, uint W, uint H)> _scissors = [];

    protected virtual Framebuffer Framebuffer { get; init; }

    public virtual void Start() {
        _commands.Begin();
        _commands.SetFramebuffer(Framebuffer);
        _commands.ClearColorTarget(0, RgbaFloat.Grey);
        ClearDepthStencil();
    }

    public virtual void End() {
        _commands.End();
        _gfx.SubmitCommands(_commands);

        _lastShader = null;
        _lastMaterial = null;
        _lastMesh = null;
    }

    public void Render<TVertex>(MeshMaterial<TVertex> model, IConstantHolder? instanceHolder) where TVertex : unmanaged {
        if (_lastShader != model.Material.Shader) {
            _lastShader = model.Material.Shader;
            _lastShader.Apply(_commands);
        }

        if (_lastMaterial != model.Material) {
            _lastMaterial = model.Material;
            _lastMaterial.ApplyTextures(_commands);
        }

        if (_lastMesh != model.Mesh) {
            _lastMesh = model.Mesh;
            _lastMesh.ApplyGeometry(_commands);
        }

        _lastShader.Constants?.UpdateAndSetBuffers(_commands, _lastShader.ConstantSlot);
        instanceHolder?.UpdateAndSetBuffers(_commands, _lastShader.InstanceConstantSlot);

        _lastMesh.Draw(_commands);
    }

    public void ClearDepthStencil() {
        _commands.ClearDepthStencil(1);
    }

    public void PushScissor(uint x, uint y, uint w, uint h) {
        _scissors.Push((x, y, w, h));
        _commands.SetScissorRect(0, x, y, w, h);
    }

    public void PopScissor() {
        if (_scissors.Count == 0) {
            return;
        }

        _scissors.Pop();
        if (_scissors.TryPeek(out var scissor)) {
            var (x, y, w, h) = scissor;
            _commands.SetScissorRect(0, x, y, w, h);
        } else {
            _commands.SetFullScissorRect(0);
        }
    }

    protected override void DisposeImpl() {
        _commands.Dispose();
    }
}
