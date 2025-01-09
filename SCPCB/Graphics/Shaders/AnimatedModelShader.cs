using ShaderGen;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Assimp;
using SCPCB.Graphics.Assimp;
using static ShaderGen.ShaderBuiltins;
using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.Shaders.Fragments;
using SCPCB.Graphics.Shaders.Utility;
using SCPCB.Graphics.Shaders.Vertices;

#pragma warning disable CS8618

namespace SCPCB.Graphics.Shaders;

public partial class AnimatedModelShader : IAssimpMaterialConvertible<AnimatedModelShader.Vertex, GraphicsResources>,
        IAutoShader<AnimatedModelShader.VertexConstants, Empty, AnimatedModelShader.InstanceVertexConstants, Empty> {

    public struct Vertex : IAssimpVertexConvertible<Vertex>, IAnimatedVertex {
        [PositionSemantic] public Vector3 Position;
        [TextureCoordinateSemantic] public Vector2 TextureCoord;
        [TextureCoordinateSemantic] public Int4 BoneIDs;
        [TextureCoordinateSemantic] public Vector4 BoneWeights;

        public static Vertex ConvertVertex(AssimpVertex vert) => new() {
            Position = vert.Position,
            TextureCoord = vert.TexCoords[0].XY(),
            BoneIDs = new(-1),
            BoneWeights = Vector4.Zero,
        };

        Int4 IAnimatedVertex.BoneIDs { get => BoneIDs; set => BoneIDs = value; }
        Vector4 IAnimatedVertex.BoneWeights { get => BoneWeights; set => BoneWeights = value; }
    }

    public struct VertexConstants : IProjectionMatrixConstantMember, IViewMatrixConstantMember {
        public Matrix4x4 ProjectionMatrix { get; set; }
        public Matrix4x4 ViewMatrix { get; set; }
    }

    public struct InstanceVertexConstants : IWorldMatrixConstantMember, IBoneTransformsConstantMember {
        // TODO: This sucks, it will continue to suck but using the InlineArrayAttribute might make it better.
        [ResourceIgnore]
        private unsafe fixed float _boneTransform[IBoneTransformsConstantMember.LENGTH * 4 * 4];
        [ArraySize(IBoneTransformsConstantMember.LENGTH), TreatAsAutoProperty]
        public unsafe Span<Matrix4x4> BoneTransforms => MemoryMarshal.CreateSpan(ref Unsafe.As<float, Matrix4x4>(ref _boneTransform[0]), IBoneTransformsConstantMember.LENGTH);
        public Matrix4x4 WorldMatrix { get; set; }
    }

    [ResourceSet(MATERIAL_OFFSET)] public Texture2DResource SurfaceTexture { get; }
    [ResourceSet(MATERIAL_OFFSET)] public SamplerResource Sampler { get; }

    [VertexShader]
    public unsafe FPositionTexture VS(Vertex input) {
        FPositionTexture output;
        Vector4 finalPos = new();
        for (int i = 0; i < 4; i++) {
            // TODO: Performance implications of the branching vs up to 4 redundant matrix multiplications?
            if (input.BoneIDs[i] != -1) {
                if (input.BoneIDs[i] >= IBoneTransformsConstantMember.LENGTH) {
                    finalPos = new(input.Position, 1);
                    break;
                }
                var localPosition = Vector4.Transform(input.Position, InstanceVertexBlock.BoneTransforms[input.BoneIDs[i]]);
                finalPos += localPosition * input.BoneWeights[i];
            }
        }
        finalPos = Mul(InstanceVertexBlock.WorldMatrix, finalPos);
        finalPos = Mul(VertexBlock.ViewMatrix, finalPos);
        output.Position = Mul(VertexBlock.ProjectionMatrix, finalPos);
        output.TextureCoord = input.TextureCoord;
        return output;
    }

    [FragmentShader]
    public Vector4 FS(FPositionTexture input) {
        return Sample(SurfaceTexture, Sampler, input.TextureCoord);
    }

    public static ICBMaterial<Vertex> ConvertMaterial(Material mat, string fileDir, GraphicsResources gfxRes)
        => gfxRes.MaterialCache.GetMaterial<AnimatedModelShader, Vertex>(
            [gfxRes.TextureCache.GetTexture(fileDir + '/' + Path.GetFileName(mat.TextureDiffuse.FilePath))],
            [gfxRes.GraphicsDevice.Aniso4xSampler]);
}
