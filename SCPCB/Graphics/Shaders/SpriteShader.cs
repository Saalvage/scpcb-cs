﻿using ShaderGen;
using System.Numerics;
using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders.ConstantMembers;
using static ShaderGen.ShaderBuiltins;
using SCPCB.Graphics.Shaders.Fragments;
using SCPCB.Graphics.Shaders.Utility;
using SCPCB.Graphics.Shaders.Vertices;

#pragma warning disable CS8618

namespace SCPCB.Graphics.Shaders;

public partial class SpriteShader : IAutoShader<SpriteShader.VertexConstants, Empty,
        SpriteShader.InstanceVertexConstants, SpriteShader.InstanceFragmentConstants> {

    public struct VertexConstants : IProjectionMatrixConstantMember, IViewMatrixConstantMember {
        public Matrix4x4 ProjectionMatrix { get; set; }
        public Matrix4x4 ViewMatrix { get; set; }
    }

    public struct InstanceVertexConstants : IWorldMatrixConstantMember {
        public Matrix4x4 WorldMatrix { get; set; }
    }

    public struct InstanceFragmentConstants : IColorAlphaConstantMember {
        public Vector4 Color { get; set; }
    }

    [ResourceSet(MATERIAL_OFFSET)] public Texture2DResource SurfaceTexture;
    [ResourceSet(MATERIAL_OFFSET)] public SamplerResource Sampler;

    [VertexShader]
    public FPositionTexture VS(VPositionTexture input) {
        FPositionTexture output;
        output.Position = Mul(VertexBlock.ProjectionMatrix,
            Mul(VertexBlock.ViewMatrix,
                Mul(InstanceVertexBlock.WorldMatrix, new(input.Position, 1))));
        output.TextureCoord = input.TextureCoord;
        return output;
    }

    [FragmentShader]
    public Vector4 FS(FPositionTexture input) {
        return Sample(SurfaceTexture, Sampler, input.TextureCoord) * InstanceFragmentBlock.Color;
    }
}
