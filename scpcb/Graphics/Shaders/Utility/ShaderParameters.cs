using Veldrid;

namespace scpcb.Graphics.Shaders.Utility;

public record struct ShaderParameters(
    BlendStateDescription BlendState,
    DepthStencilStateDescription DepthState,
    RasterizerStateDescription RasterizerState,
    PrimitiveTopology Topology) {
    public static readonly ShaderParameters Default = new(BlendStateDescription.SingleAlphaBlend,
        DepthStencilStateDescription.DepthOnlyLessEqual, RasterizerStateDescription.Default with { FrontFace = FrontFace.CounterClockwise },
        PrimitiveTopology.TriangleList);
}
