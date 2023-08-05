using System.Drawing;
using System.Numerics;
using System.Text.RegularExpressions;
using BepuPhysics.Collidables;
using scpcb.Graphics;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders;
using scpcb.Physics;
using scpcb.Utility;

namespace scpcb.RoomProviders;

public partial class RMeshRoomProvider : IRoomProvider {
    public IEnumerable<string> SupportedExtensions { get; } = new[] { "rmesh" };

    public RoomData LoadRoom(GraphicsResources gfxRes, PhysicsResources physRes, string filename) {
        using var fileHandle = File.OpenRead(filename);
        using var reader = new BinaryReader(fileHandle);

        // TODO: Tweak values?
        Span<RMeshShader.Vertex> vertexStackBuffer = stackalloc RMeshShader.Vertex[512];
        Span<uint> indexStackBuffer = stackalloc uint[1024];

        var vertexHeapBuffer = Array.Empty<RMeshShader.Vertex>();
        var indexHeapBuffer = Array.Empty<uint>();

        var hasTriggerBox = false;
        switch (reader.ReadB3DString()) {
            case "RoomMesh":
                break;
            case "RoomMesh.HasTriggerBox":
                hasTriggerBox = true;
                break;
            default:
                throw new ArgumentException($"{filename} is not a valid .rmesh file!");
        }

        var shader = gfxRes.ShaderCache.GetShader<RMeshShaderGenerated>();
        var constants = shader.TryCreateInstanceConstants(); // Shared constants for all meshes.
        var meshCount = reader.ReadInt32();
        var meshes = new ICBModel[meshCount];
        // TODO: Estimate number of tris per mesh better
        physRes.BufferPool.TakeAtLeast<Triangle>(meshCount * 100, out var triBuffer);
        try {
            var totalTriCount = 0;
            for (var i = 0; i < meshCount; i++) {
                var textures = new ICBTexture?[2];

                var isOpaque = true;
                for (var j = 0; j < 2; j++) {
                    var textureFlags = reader.ReadByte();
                    if (textureFlags == 0) { continue; }
                    var textureFile = reader.ReadB3DString();
                    if (textureFile == "") { continue; }
                    var hasAlpha = textureFlags >= 3; // whether the file should be loaded with alpha channel, we ignore this

                    var isLightmap = LmRegex().IsMatch(textureFile);
                    var fileLocation = isLightmap
                        ? Path.Combine(Path.GetDirectoryName(filename), textureFile)
                        : "Assets/Textures/" + textureFile;
                    if (!File.Exists(fileLocation)) {
                        Console.WriteLine($"Texture {fileLocation} not found!");
                        continue;
                    }
                    textures[isLightmap ? 0 : 1] = gfxRes.TextureCache.GetTexture(fileLocation);
                    // We cannot use the texture itself for this since it
                    // correlates with level geometry as well.
                    if (!isLightmap && textureFlags == 3) {
                        isOpaque = false;
                    }
                }

                textures[0] ??= gfxRes.TextureCache.GetTexture(Color.White);
                textures[1] ??= gfxRes.MissingTexture;

                var mat = shader.CreateMaterial(textures!);

                var vertexCount = reader.ReadInt32();
                var vertices = GetBufferedSpan(vertexCount, vertexStackBuffer, ref vertexHeapBuffer);
                for (var j = 0; j < vertices.Length; j++) {
                    var pos = reader.ReadVector3();
                    pos.X = -pos.X;
                    var uv1 = reader.ReadVector2();

                    var uv2 = reader.ReadVector2();

                    var r = reader.ReadByte() / 255f;
                    var g = reader.ReadByte() / 255f;
                    var b = reader.ReadByte() / 255f;
                    vertices[j] = new(pos / 100, uv1, uv2, new(r, g, b));
                }

                var triangleCount = reader.ReadInt32() * (isOpaque ? 1 : 2);
                var stride = isOpaque ? 3 : 6;
                physRes.BufferPool.ResizeToAtLeast(ref triBuffer, totalTriCount + triangleCount, totalTriCount);
                var indices = GetBufferedSpan(triangleCount * 3, indexStackBuffer, ref indexHeapBuffer);
                for (var j = 0; j < indices.Length; j += stride) {
                    var i1 = indices[j + 2] = reader.ReadUInt32();
                    var i2 = indices[j + 1] = reader.ReadUInt32();
                    var i3 = indices[j + 0] = reader.ReadUInt32();
                    if (!isOpaque) {
                        var i4 = indices[j + 3] = i1;
                        var i5 = indices[j + 4] = i2;
                        var i6 = indices[j + 5] = i3;
                        triBuffer[totalTriCount] = new(vertices[(int)i6].Position, vertices[(int)i5].Position, vertices[(int)i4].Position);
                        totalTriCount++;
                    }
                    triBuffer[totalTriCount] = new(vertices[(int)i1].Position, vertices[(int)i2].Position, vertices[(int)i3].Position);
                    totalTriCount++;
                }

                // TODO: Transparency does not work entirely correctly if we bunch it all up in a single mesh.
                meshes[i] = new CBModel<RMeshShader.Vertex>(constants, mat,
                    new CBMesh<RMeshShader.Vertex>(gfxRes.GraphicsDevice, vertices, indices), isOpaque);
            }

            return new(meshes, Mesh.CreateWithSweepBuild(triBuffer[..totalTriCount], Vector3.One, physRes.BufferPool));
        } finally {
            physRes.BufferPool.Return(ref triBuffer);
        }
    }

    [GeneratedRegex(@"_lm\d+\.\w+$", RegexOptions.IgnoreCase)]
    private static partial Regex LmRegex();

    private Span<T> GetBufferedSpan<T>(int count, Span<T> stackBuffer, ref T[] heapBuffer)
        => count <= stackBuffer.Length ? stackBuffer[..count]
        : new(count <= heapBuffer.Length
            ? heapBuffer
            : heapBuffer = new T[count],
        0, count);
}
