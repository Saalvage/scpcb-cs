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

public partial class BillboardShader : IAutoShader<BillboardShader.VertexConstants, Empty,
        BillboardShader.InstanceVertexConstants, BillboardShader.InstanceFragmentConstants> {

    public struct VertexConstants : IProjectionMatrixConstantMember, IViewMatrixConstantMember, IViewPositionConstantMember {
        public Matrix4x4 ProjectionMatrix { get; set; }
        public Matrix4x4 ViewMatrix { get; set; }
        public Vector3 ViewPosition { get; set; }
    }

    public struct InstanceVertexConstants : IWorldMatrixConstantMember {
        public Matrix4x4 WorldMatrix { get; set; }
    }

    public struct InstanceFragmentConstants : IColorAlphaConstantMember {
        public Vector4 Color { get; set; }
    }

    [ResourceSet(MATERIAL_OFFSET)] public Texture2DResource SurfaceTexture;
    [ResourceSet(MATERIAL_OFFSET)] public SamplerResource Sampler;

    private static Matrix4x4 CreateLookAt(Vector3 cameraDirection, Vector3 cameraUpVector) {
        Vector3 axisZ = Vector3.Normalize(-cameraDirection);
        Vector3 axisX = Vector3.Normalize(Vector3.Cross(cameraUpVector, axisZ));
        Vector3 axisY = Vector3.Cross(axisZ, axisX);

        Matrix4x4 result;

        result.M11 = axisX.X;
        result.M12 = axisY.X;
        result.M13 = axisZ.X;
        result.M14 = 0;

        result.M21 = axisX.Y;
        result.M22 = axisY.Y;
        result.M23 = axisZ.Y;
        result.M24 = 0;

        result.M31 = axisX.Z;
        result.M32 = axisY.Z;
        result.M33 = axisZ.Z;
        result.M34 = 0;

        result.M41 = 0;
        result.M42 = 0;
        result.M43 = 0;
        result.M44 = 1;

        return result;
    }

    [VertexShader]
    public FPositionTexture VS(VPositionTexture input) {
        // TODO: Multiple things to be considered here:
        // - It seems like the matrices get transposed on their way to the GPU (at least the way we access them seems inverted)
        // - Support for stuff like Matrix4x4.CreateLookAt would be neat.
        // - Support for Matrix3x3 would be neat (probably as an addon lib).
        // - Support for QUATERNIONS would be awesome!
        // - Pick one of the above for the transforms here.
        // - Don't forget fog!

        // Extracting and removing world pos. Allows for rotating the billboards.
        var world = InstanceVertexBlock.WorldMatrix;
        var entityWorldPos = new Vector3(world.M14, world.M24, world.M34) / world.M44;
        world.M14 = 0;
        world.M24 = 0;
        world.M34 = 0;

        Matrix4x4 mat = CreateLookAt(VertexBlock.ViewPosition - entityWorldPos, new(0, 1, 0));

        // Rotation & scale.
        Vector4 preWorldPosition = Mul(world, new(input.Position, 1));
        // Look at & translation.
        Vector4 worldPosition = Mul(mat, preWorldPosition) + new Vector4(entityWorldPos, 0);
        Vector4 viewPosition = Mul(VertexBlock.ViewMatrix, worldPosition);

        FPositionTexture output;
        output.Position = Mul(VertexBlock.ProjectionMatrix, viewPosition);
        output.TextureCoord = input.TextureCoord;
        return output;
    }

    [FragmentShader]
    public Vector4 FS(FPositionTexture input) {
        return Sample(SurfaceTexture, Sampler, input.TextureCoord) * InstanceFragmentBlock.Color;
    }
}
