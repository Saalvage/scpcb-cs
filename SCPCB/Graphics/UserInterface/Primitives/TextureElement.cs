using System.Drawing;
using System.Numerics;
using SCPCB.Graphics.Caches;
using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.Textures;
using SCPCB.Utility;

namespace SCPCB.Graphics.UserInterface.Primitives;

public class TextureElement : UIElement, ISharedMeshProvider<TextureElement, UIShader.Vertex>, IColorizableElement {
    private readonly MeshInstance<UIShader.Vertex> _meshInstance;

    public ICBTexture Texture { get; }

    public Color Color { get; set; } = Color.White;
    public Vector2 UvOffset { get; set; } = Vector2.Zero;
    public Vector2 UvSize { get; set; } = Vector2.One;

    private Vector2 _rotationSinCos;
    public float RotationDegrees {
        set {
            var rad = value * MathF.PI / 180;
            _rotationSinCos = new(MathF.Sin(rad), MathF.Cos(rad));
        }
        get => MathF.Asin(_rotationSinCos.X);
    }

    public TextureElement(GraphicsResources gfxRes, ICBTexture texture, bool tile = false) {
        _meshInstance = new(null,
            gfxRes.MaterialCache.GetMaterial<UIShader, UIShader.Vertex>([texture], [tile ? gfxRes.WrapAnisoSampler : gfxRes.ClampAnisoSampler]),
            gfxRes.MeshCache.GetMesh<TextureElement, UIShader.Vertex>());
        Texture = texture;
        PixelSize = new(texture.Width, texture.Height);
        RotationDegrees = 0;
    }

    protected override void DrawInternal(IRenderTarget target, Vector2 position, float z) {
        _meshInstance.Material.Shader.Constants!.SetValue<IPositionConstantMember, Vector3>(new(position, Z + z));
        _meshInstance.Material.Shader.Constants!.SetValue<IUIScaleConstantMember, Vector2>(PixelSize);
        _meshInstance.Material.Shader.Constants!.SetValue<IColorAlphaConstantMember, Vector4>(Color.ToRGBA());
        var uvPositionEnd = UvOffset + UvSize;
        _meshInstance.Material.Shader.Constants!.SetValue<ITexCoordsConstantMember, Vector4>(new(UvOffset.X, uvPositionEnd.X,
            UvOffset.Y, uvPositionEnd.Y));
        _meshInstance.Material.Shader.Constants!.SetValue<IRotation2DConstantMember, Vector2>(_rotationSinCos);
        _meshInstance.Render(target, 0f);
    }

    public static ICBMesh<UIShader.Vertex> CreateSharedMesh(GraphicsResources gfxRes)
        => new CBMesh<UIShader.Vertex>(gfxRes.GraphicsDevice, [
            new(new(-0.5f, -0.5f)),
            new(new(0.5f, -0.5f)),
            new(new(-0.5f, 0.5f)),
            new(new(0.5f, 0.5f)),
        ], [2, 1, 0, 1, 2, 3]);
}
