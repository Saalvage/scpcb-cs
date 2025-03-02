using System.Drawing;
using System.Numerics;
using SCPCB.Entities;
using SCPCB.Graphics.Shaders;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.Shaders.Utility;
using SCPCB.Graphics.Shaders.Vertices;
using SCPCB.Utility;

namespace SCPCB.Graphics.Text;

public class TextModel3D : Disposable, ISortableMeshInstanceHolder, ITransformable, IConstantProvider<IColorConstantMember, Vector3> {
    public Transform WorldTransform { get; set; } = new();

    public Color Color { get; set; } = Color.White;

    IEnumerable<ISortableMeshInstance> ISortableMeshInstanceHolder.Instances => Sortables;
    public IReadOnlyList<ISortableMeshInstance> Sortables { get; }

    private readonly TextModel _text;

    public TextModel3D(GraphicsResources gfxRes, Font font, string text) {
        _text = new(gfxRes, font, gfxRes.ShaderCache.GetShader<TextShader3D, VPositionTexture2D>());
        _text.Text = text;
        Sortables = _text.Meshes.Select(x => new DependentSortableMeshInstance(x, this, false)).ToArray();
        foreach (var i in _text.Meshes) {
            // TODO: The question poses itself again, do all mesh instances need us in their constant providers?
            // Maybe the constant holder itself simply need to be able to store a SET of constant providers??
            i.ConstantProviders.Add(this);
        }
    }

    protected override void DisposeImpl() {
        _text.Dispose();
    }

    Vector3 IConstantProvider<IColorConstantMember, Vector3>.GetValue(float interp)
        => Color.ToRGB();

    public void ApplyTo(ReadOnlySpan<IConstantHolder?> holders, float interp) {
        var off = -_text.Dimensions / 2;
        off.Y = -off.Y;
        var trans = WorldTransform + new Transform(new(off, 0), Quaternion.Identity);
        var transMat = trans.GetMatrix();
        foreach (var holder in holders) {
            holder?.TrySetValue<IWorldMatrixConstantMember, Matrix4x4>(transMat);
            holder?.TrySetValue<IColorConstantMember, Vector3>(Color.ToRGB());
        }
    }
}
