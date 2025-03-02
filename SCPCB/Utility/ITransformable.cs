using System.Numerics;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.Shaders.Utility;

namespace SCPCB.Utility;

// This approach to parenting/hierarchies is intended to be sufficient for our purposes while remaining non-intrusive.
// I don't really think we currently have any use cases for deeply nested hierarchies.
// As such, performance is O(N^2) at worst, while having (practically) no overhead for a flat hierarchy.
// O(N) is possible, and if we do come to utilize deep hierarchies the previous approach may give some pointers:
// https://github.com/Saalvage/scpcb-cs/blob/b91be64/SCPCB/Utility/HierarchicalTransform.cs
public interface ITransformable : IConstantProvider<IWorldMatrixConstantMember, Matrix4x4>, IPositioned {
    Transform WorldTransform { get; set; }
    Transform GetInterpolatedWorldTransform(float interp) => WorldTransform;
    Matrix4x4 IConstantProvider<IWorldMatrixConstantMember, Matrix4x4>.GetValue(float interp) => GetInterpolatedWorldTransform(interp).GetMatrix();
    Vector3 IPositioned.Position => WorldTransform.Position;
}

// TODO: This currently can't be used as much as it should be, because the default implemented properties aren't visible on the implementing classes.
// I think the most general solution would be having attributes in the interfaces and a source generator for the implementing classes.
public interface IParentableTransformable : ITransformable {
    ITransformable? Parent { get; set; }

    Transform LocalTransform { get; set; }

    Transform GetInterpolatedLocalTransform(float interp) => LocalTransform;

    Transform ITransformable.WorldTransform {
        get => (Parent?.WorldTransform ?? new Transform()) + LocalTransform;
        set => LocalTransform = value - (Parent?.WorldTransform ?? new Transform());
    }

    Transform ITransformable.GetInterpolatedWorldTransform(float interp)
        => (Parent?.GetInterpolatedWorldTransform(interp) ?? new Transform()) + GetInterpolatedLocalTransform(interp);
}
