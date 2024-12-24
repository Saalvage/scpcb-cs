using System.Numerics;

namespace SCPCB.Graphics.Shaders.ConstantMembers;

public interface ITexCoordsConstantMember : IConstantMember<ITexCoordsConstantMember, Vector4> {
    public Vector4 TexCoords { get; set; }

    Vector4 IConstantMember<ITexCoordsConstantMember, Vector4>.Value {
        set => TexCoords = value;
    }
}
