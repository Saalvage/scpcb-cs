using scpcb.Entities;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Graphics.Shaders;
using scpcb.Utility;
using System.Numerics;
using scpcb.Graphics.Shaders.Utility;

namespace scpcb.Graphics;

public class Billboard : I3DModel, IConstantProvider<IWorldMatrixConstantMember, Matrix4x4>, IConstantProvider<IColorConstantMember, Vector3> {
    Vector3 I3DModel.Position => Transform.Position;
    public Transform Transform { get; set; } = new();

    public Vector3 Color { get; set; } = Vector3.One;

    public ICBModel Model { get; }

    /// <summary>
    /// Do not call this directly, use a <see cref="BillboardManager"/> instead.
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="mat"></param>
    public Billboard(ICBMesh<BillboardShader.Vertex> mesh, ICBMaterial<BillboardShader.Vertex> mat) {
        Model = new CBModel<BillboardShader.Vertex>(mat.Shader.TryCreateInstanceConstants(),
            mat, mesh, false);
        Model.ConstantProviders.Add(this);
    }

    public Matrix4x4 GetValue(float interp)
        => Transform.GetMatrix();

    Vector3 IConstantProvider<IColorConstantMember, Vector3>.GetValue(float interp) => Color;

    public void ApplyTo(IEnumerable<IConstantHolder?> holders, float interp) {
        ((IConstantProvider<IWorldMatrixConstantMember, Matrix4x4>)this).ApplyToInternal(holders, interp);
        ((IConstantProvider<IColorConstantMember, Vector3>)this).ApplyToInternal(holders, interp);
    }
}
