﻿using System.Numerics;
using SCPCB.Entities;
using SCPCB.Graphics.Caches;
using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.Shaders.Utility;
using SCPCB.Graphics.Shaders.Vertices;
using SCPCB.Utility;
using Veldrid;

namespace SCPCB.Graphics;

public class Billboard : I3DModel, IConstantProvider<IWorldMatrixConstantMember, Matrix4x4>, IConstantProvider<IColorAlphaConstantMember, Vector4>,
        ISharedMeshProvider<Billboard, VPositionTexture> {
    Vector3 I3DEntity.Position => Transform.Position;
    public Transform Transform { get; set; } = new();

    public Vector4 Color { get; set; } = Vector4.One;

    public ICBModel Model { get; }

    public bool IsOpaque => false;

    /// <summary>
    /// Do not call this directly, use a <see cref="BillboardManager"/> instead.
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="mat"></param>
    public Billboard(ICBMesh<VPositionTexture> mesh, ICBMaterial<VPositionTexture> mat) {
        Model = new CBModel<VPositionTexture>(mat.Shader.TryCreateInstanceConstants(), mat, mesh);
        Model.ConstantProviders.Add(this);
    }

    public static Billboard Create(GraphicsResources gfxRes, ICBTexture texture,
            bool shouldLookAt = false, bool additiveBlend = false, bool shouldRenderOnTop = false) {

        Func<ShaderParameters, ShaderParameters> depthStencilModifier =
            shouldRenderOnTop ? x => x with { DepthState = DepthStencilStateDescription.Disabled } : x => x;

        var modifier = additiveBlend
            ? x => depthStencilModifier(x) with { BlendState = BlendStateDescription.SingleAdditiveBlend }
            : depthStencilModifier;

        var shader = shouldLookAt ? gfxRes.ShaderCache.GetShader<BillboardShader, VPositionTexture>(modifier)
            : gfxRes.ShaderCache.GetShader<SpriteShader, VPositionTexture>(modifier);
        var mat = gfxRes.MaterialCache.GetMaterial(shader, [texture], [gfxRes.ClampAnisoSampler]);
        return new(gfxRes.MeshCache.GetMesh<Billboard, VPositionTexture>(), mat);
    }

    public Matrix4x4 GetValue(float interp)
        => Transform.GetMatrix();

    Vector4 IConstantProvider<IColorAlphaConstantMember, Vector4>.GetValue(float interp) => Color;

    public void ApplyTo(IEnumerable<IConstantHolder?> holders, float interp) {
        ((IConstantProvider<IWorldMatrixConstantMember, Matrix4x4>)this).ApplyToInternal(holders, interp);
        ((IConstantProvider<IColorAlphaConstantMember, Vector4>)this).ApplyToInternal(holders, interp);
    }

    public static ICBMesh<VPositionTexture> CreateSharedMesh(GraphicsResources gfxRes)
        => new CBMesh<VPositionTexture>(gfxRes.GraphicsDevice, new VPositionTexture[] {
                new(new(-1f, 1f, 0), new(1, 0)),
                new(new(1f, 1f, 0), new(0, 0)),
                new(new(-1f, -1f, 0), new(1, 1)),
                new(new(1f, -1f, 0), new(0, 1)),
            },
            [0, 1, 2, 3, 2, 1]);
}