using SCPCB.Graphics.Shaders.ConstantMembers;
using System.Numerics;
using SCPCB.Entities;

namespace SCPCB.Graphics;

public class FogController : IEntity {
    private readonly GraphicsResources _gfxRes;

    /// <remarks>
    /// In degrees.
    /// </remarks>
    public float FieldOfView {
        get;
        set {
            field = value;
            UpdateProjection();
        }
    } = 90;

    public float Near {
        get;
        set {
            field = value;
            UpdateFog();
        }
    } = 0;

    public float Far {
        get;
        set {
            field = value;
            UpdateProjection();
            UpdateFog();
        }
    } = float.PositiveInfinity;

    public FogController(GraphicsResources gfxRes) {
        _gfxRes = gfxRes;
    }

    private void UpdateProjection() {
        _gfxRes.ShaderCache.SetGlobal<IProjectionMatrixConstantMember, Matrix4x4>(
            Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 180 * FieldOfView, (float)_gfxRes.Window.Width / _gfxRes.Window.Height,
                0.1f, Far));
    }

    private void UpdateFog() {
        _gfxRes.ShaderCache.SetGlobal<IFogRangeConstantMember, Fog>(new(Near, Far));
    }
}
