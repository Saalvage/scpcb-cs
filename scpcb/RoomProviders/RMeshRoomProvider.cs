using System.Numerics;
using System.Runtime.InteropServices;
using scpcb.Collision;
using scpcb.Graphics;
using scpcb.Graphics.Shaders;

namespace scpcb.RoomProviders; 

public class RMeshRoomProvider : IRoomProvider {
    public string[] SupportedExtensions { get; } = { "rmesh" };

    public RoomInfo LoadRoom(string filename, GraphicsResources gfxRes) {
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

        var meshCount = reader.ReadInt32();
        var meshes = new ICBMesh[meshCount];
        var collisionMeshes = new CollisionMesh[meshCount];
        for (var i = 0; i < meshCount; i++) {
            ICBMaterial<RMeshShader.Vertex> mat = null;

            for (var j = 0; j < 2; j++) {
                var lightmapFlags = reader.ReadByte();
                if (lightmapFlags == 0) { continue; }
                var lightmapFile = reader.ReadB3DString();
                if (lightmapFile == "") { continue; } // Is this really correct? .rmesh sucks balls!
                var hasAlpha = lightmapFlags >= 3; // whether the file should be loaded with alpha channel
                // LOAD TEXTURE lightmap file
                // if lightmapFlags == 1 => Multiply 2 blend mode
                // if texture contains (_lm) => Additive blend mode
                var fileLocation = "Assets/Textures/" + lightmapFile;
                if (j == 1) {
                    if (!File.Exists(fileLocation)) {
                        Console.WriteLine($"Texture {lightmapFile} not found!");
                        fileLocation = "Assets/Textures/Missing.png";
                    }
                    Console.WriteLine(fileLocation + "  " + lightmapFlags);
                    mat = gfxRes.ShaderCache.GetShader<RMeshShaderGenerated>().CreateMaterial(gfxRes.TextureCache.GetTexture(fileLocation));
                }
            }

            var vertexCount = reader.ReadInt32();
            var vertices = GetBufferedSpan(vertexCount, vertexStackBuffer, ref vertexHeapBuffer);
            for (var j = 0; j < vertices.Length; j++) {
                var pos = reader.ReadVector3();
                pos.X = -pos.X;
                var uv1 = reader.ReadVector2();

                var uv2 = reader.ReadVector2();

                vertices[j] = new(pos / 1000f, uv1);
                reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();
            }

            var triangleCount = reader.ReadInt32();
            var indices = GetBufferedSpan(triangleCount * 3, indexStackBuffer, ref indexHeapBuffer);
            for (var j = indices.Length - 1; j >= 0; j--) {
                indices[j] = reader.ReadUInt32();
            }

            meshes[i] = new CBMesh<RMeshShader.Vertex>(gfxRes.GraphicsDevice, mat, vertices, indices);
            var collVerts = new Vector3[vertices.Length];
            for (var k = 0; k < collVerts.Length; k++) {
                collVerts[k] = vertices[k].Position * 1000;
            }
            collisionMeshes[i] = new(collVerts, indices.ToArray());
        }

        return new(meshes, collisionMeshes);
    }

    private Span<T> GetBufferedSpan<T>(int count, Span<T> stackBuffer, ref T[] heapBuffer)
        => count <= stackBuffer.Length ? stackBuffer[..count]
        : new (count <= heapBuffer.Length
            ? heapBuffer
            : heapBuffer = new T[count],
        0, count);
}
