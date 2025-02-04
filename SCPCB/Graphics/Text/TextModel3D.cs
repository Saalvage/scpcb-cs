using System.Drawing;
using System.Numerics;
using SCPCB.Entities;
using SCPCB.Graphics.Shaders;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.Shaders.Utility;
using SCPCB.Graphics.Shaders.Vertices;
using SCPCB.Utility;

namespace SCPCB.Graphics.Text;

public class TextModel3D : Disposable, ISortableMeshInstanceHolder, IConstantProvider<IWorldMatrixConstantMember, Matrix4x4>, IConstantProvider<IColorConstantMember, Vector3> {
    // TODO: This is basically copied from Model. We should find a common interface for a dependant ISortableMeshInstance.
    // It'd be cheeky to use IConstantProvider<IWorldMatrixConstantMember, Matrix4x4> as the required interface and extract
    // the position from the matrix.
    private class TextMeshInstance : ISortableMeshInstance {
        private readonly TextModel3D _model;
        public Vector3 Position => _model.WorldTransform.Position;
        public IMeshInstance MeshInstance { get; }
        public bool IsOpaque => false;

        public TextMeshInstance(TextModel3D model, IMeshInstance instance) {
            _model = model;
            MeshInstance = instance;
        }
    }

    // TODO: What if we have a 3D text we want to move every tick and interpolate in between?
    // We need a generic solution for this.
    public Transform WorldTransform { get; set; } = new();

    public Color Color { get; set; } = Color.White;

    IEnumerable<ISortableMeshInstance> ISortableMeshInstanceHolder.Instances => Sortables;
    public IReadOnlyList<ISortableMeshInstance> Sortables { get; }

    private readonly TextModel _text;

    public TextModel3D(GraphicsResources gfxRes, Font font, string text) {
        _text = new(gfxRes, font, gfxRes.ShaderCache.GetShader<TextShader3D, VPositionTexture2D>());
        _text.Text = text;
        Sortables = _text.Meshes.Select(x => new TextMeshInstance(this, x)).ToArray();
        foreach (var i in _text.Meshes) {
            // TODO: The question poses itself again, do all mesh instances need us in their constant providers?
            // Maybe the constant holder itself simply need to be able to store a SET of constant providers??
            i.ConstantProviders.Add(this);
        }
    }

    protected override void DisposeImpl() {
        _text.Dispose();
    }

    Matrix4x4 IConstantProvider<IWorldMatrixConstantMember, Matrix4x4>.GetValue(float interp)
        => WorldTransform.GetMatrix();

    Vector3 IConstantProvider<IColorConstantMember, Vector3>.GetValue(float interp)
        => new Vector3(Color.R, Color.G, Color.B) / 255f;

    public void ApplyTo(ReadOnlySpan<IConstantHolder?> holders, float interp) {
        foreach (var holder in holders) {
            holder?.TrySetValue<IWorldMatrixConstantMember, Matrix4x4>(WorldTransform.GetMatrix());
            holder?.TrySetValue<IColorConstantMember, Vector3>(new Vector3(Color.R, Color.G, Color.B) / 255f);
        }
    }
}

