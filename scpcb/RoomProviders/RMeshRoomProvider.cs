using System.Diagnostics;
using scpcb.Graphics.Shaders;
using scpcb.Shaders;
using Veldrid;

namespace scpcb.RoomProviders; 

public class RMeshRoomProvider : IRoomProvider {
    public string[] SupportedExtensions { get; } = { "rmesh" };

    public List<CBMesh<ModelShader.Vertex>> Test(string filename, GraphicsDevice gfx, ICBMaterial<ModelShader.Vertex> mat) {
        using var fileHandle = File.OpenRead(filename);
        using var reader = new BinaryReader(fileHandle);

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

        List<CBMesh<ModelShader.Vertex>> a = new();
        var meshCount = reader.ReadInt32();
        for (var i = 0; i < meshCount; i++) {
            for (var j = 0; j < 2; j++) {
                var lightmapFlags = reader.ReadByte();
                if (lightmapFlags == 0) { continue; }
                var lightmapFile = reader.ReadB3DString();
                if (lightmapFile == "") { continue; } // This does not seem to correct.
                var hasAlpha = lightmapFlags >= 3; // whether the file should be loaded with alpha channel
                // LOAD TEXTURE lightmap file
                // if lightmapFlags == 1 => Multiply 2 blend mode
                // if texture contains (_lm) => Additive blend mode
                Console.WriteLine(j + " " + lightmapFile);
            }

            var vertices = Enumerable.Range(0, reader.ReadInt32())
                .Select(_ => {
                    var ret = new ModelShader.Vertex(reader.ReadVector3() / 1000f, reader.ReadVector2());
                    reader.ReadVector2();
                    reader.ReadByte();
                    reader.ReadByte();
                    reader.ReadByte();
                    return ret;
                })
                .ToArray();

            var triangleCount = reader.ReadInt32();
            var indices = Enumerable.Range(0, triangleCount)
                .SelectMany(_ => Enumerable.Repeat(0, 3).Select(_ => (uint)reader.ReadInt32()))
                .ToArray();

            a.Add(new(gfx, mat, vertices, indices));
        }

        return a;
    }
}
