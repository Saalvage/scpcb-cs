using scpcb.Graphics;
using scpcb.Graphics.Shaders;

namespace scpcb.RoomProviders; 

public class RMeshRoomProvider : IRoomProvider {
    public string[] SupportedExtensions { get; } = { "rmesh" };

    public List<CBMesh<RMeshShader.Vertex>> Test(string filename, GraphicsResources gfxRes) {
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

        List<CBMesh<RMeshShader.Vertex>> meshes = new();
        var meshCount = reader.ReadInt32();
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
                    mat = gfxRes.ShaderCache.GetShader<RMeshShader>().CreateMaterial(gfxRes.TextureCache.GetTexture(fileLocation));
                }
            }

            var vertexCount = reader.ReadInt32();
            var vertices = GetBufferedSpan(vertexCount, vertexStackBuffer, ref vertexHeapBuffer);
            for (var j = 0; j < vertices.Length; j++) {
                var pos = reader.ReadVector3();
                var uv1 = reader.ReadVector2();
                //uv1.X = 1f - uv1.X; TODO: Fix UVs

                var uv2 = reader.ReadVector2();

                vertices[j] = new(pos / 1000f, uv1);
                reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();
            }

            var triangleCount = reader.ReadInt32();
            var indices = GetBufferedSpan(triangleCount * 3, indexStackBuffer, ref indexHeapBuffer);
            for (var j = 0; j < indices.Length; j++) {
                indices[j] = reader.ReadUInt32();
            }

            meshes.Add(new(gfxRes.GraphicsDevice, mat, vertices, indices));
        }

        return meshes;
    }

    private Span<T> GetBufferedSpan<T>(int count, Span<T> stackBuffer, ref T[] heapBuffer)
        => count <= stackBuffer.Length ? stackBuffer[..count]
        : new (count <= heapBuffer.Length
            ? heapBuffer
            : heapBuffer = new T[count],
        0, count);
}
