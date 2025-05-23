﻿using System.Numerics;
using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.Shaders.Utility;
using ShaderGen;
using static ShaderGen.ShaderBuiltins;

#pragma warning disable CS8618

namespace SCPCB.Graphics.Shaders;

public partial class RMeshShader : IAutoShader<RMeshShader.VertUniforms, Empty, RMeshShader.VertInstanceUniforms, Empty> {

    public record struct Vertex([PositionSemantic] Vector3 Position, [TextureCoordinateSemantic] Vector2 Uv, [TextureCoordinateSemantic] Vector2 LmUv, [ColorSemantic] Vector3 Color);
    public record struct Fragment([SystemPositionSemantic] Vector4 Position, [TextureCoordinateSemantic] Vector2 Uv, [TextureCoordinateSemantic] Vector2 LmUv, [ColorSemantic] Vector3 Color, [TextureCoordinateSemantic] Vector3 CameraPos);

    public struct VertUniforms : IViewMatrixConstantMember, IProjectionMatrixConstantMember {
        public Matrix4x4 ViewMatrix { get; set; }
        public Matrix4x4 ProjectionMatrix { get; set; }
    }

    public struct VertInstanceUniforms : IWorldMatrixConstantMember {
        public Matrix4x4 WorldMatrix { get; set; }
    }

    [ResourceSet(MATERIAL_OFFSET)] public Texture2DResource LightmapTexture;
    [ResourceSet(MATERIAL_OFFSET)] public Texture2DResource SurfaceTexture;
    [ResourceSet(MATERIAL_OFFSET)] public SamplerResource Sampler;

    [VertexShader]
    public Fragment VS(Vertex vert) {
        Fragment frag = default;
        frag.Position = Mul(VertexBlock.ViewMatrix,
            Mul(InstanceVertexBlock.WorldMatrix, new(vert.Position, 1)));
        frag.CameraPos = frag.Position.XYZ() / frag.Position.W;
        frag.Position = Mul(VertexBlock.ProjectionMatrix, frag.Position);
        frag.Uv = vert.Uv;
        frag.LmUv = vert.LmUv;
        frag.Color = vert.Color;
        return frag;
    }

    [FragmentShader]
    public Vector4 FS(Fragment frag) {
        var based = Sample(SurfaceTexture, Sampler, frag.Uv) * new Vector4(frag.Color, 1f) * Sample(LightmapTexture, Sampler, frag.LmUv);
        var depthBetter = (frag.CameraPos.Length() - 5f)/ (20f - 5f);
        // TODO: Reconsider the branching. It has performance implications.
        return new((1 - depthBetter) * based.XYZ() * (based.W == 1f ? 2f : 1f), based.W);
    }
}
