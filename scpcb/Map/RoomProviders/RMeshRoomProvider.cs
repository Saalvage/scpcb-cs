using System.Drawing;
using System.Numerics;
using System.Text.RegularExpressions;
using BepuPhysics.Collidables;
using scpcb.Graphics;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders;
using scpcb.Map.Entities;
using scpcb.Physics;
using scpcb.Scenes;
using scpcb.Utility;
using Serilog;

namespace scpcb.Map.RoomProviders;

public partial class RMeshRoomProvider : IRoomProvider {
    public const float ROOM_SCALE_OLD = 8f / 2048f;
    public const float ROOM_SCALE = 1f / 100f;

    public IEnumerable<string> SupportedExtensions { get; } = new[] { "rmesh" };

    public IRoomData LoadRoom(IScene scene, GraphicsResources gfxRes, PhysicsResources physics, string filename) {
        object[] globals = [scene, gfxRes, physics];

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

        var shader = gfxRes.ShaderCache.GetShader<RMeshShader>();
        var meshCount = reader.ReadInt32();
        var meshes = new List<RoomData.MeshInfo>();
        meshes.EnsureCapacity(meshCount);
        // TODO: Estimate number of tris per mesh better
        physics.BufferPool.TakeAtLeast<Triangle>(meshCount * 100, out var triBuffer);
        physics.BufferPool.TakeAtLeast<Triangle>(meshCount * 5, out var invisTriBuffer);
        try {
            var totalTriCount = 0;
            var totalInvisTriCount = 0;
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
                        : "Assets/Textures/Map/" + textureFile;
                    if (!File.Exists(fileLocation)) {
                        Log.Warning("Texture {FileLocation} not found!", fileLocation);
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

                var mat = gfxRes.MaterialCache.GetMaterial(shader, textures!, gfxRes.GraphicsDevice.Aniso4xSampler.AsEnumerableElement());

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
                    vertices[j] = new(pos * ROOM_SCALE, uv1, uv2, new(r, g, b));
                }

                ref var thisTriBuffer = ref isOpaque ? ref triBuffer : ref invisTriBuffer;
                ref var thisTotalTriCount = ref isOpaque ? ref totalTriCount : ref totalInvisTriCount;

                var triCount = reader.ReadInt32() * (isOpaque ? 1 : 2);
                var stride = isOpaque ? 3 : 6;
                physics.BufferPool.ResizeToAtLeast(ref thisTriBuffer, totalTriCount + triCount, totalTriCount);
                var indices = GetBufferedSpan(triCount * 3, indexStackBuffer, ref indexHeapBuffer);
                for (var j = 0; j < indices.Length; j += stride) {
                    var i1 = indices[j + 2] = (uint)reader.ReadInt32();
                    var i2 = indices[j + 1] = (uint)reader.ReadInt32();
                    var i3 = indices[j + 0] = (uint)reader.ReadInt32();
                    if (!isOpaque) {
                        var i4 = indices[j + 3] = i1;
                        var i5 = indices[j + 4] = i2;
                        var i6 = indices[j + 5] = i3;
                        thisTriBuffer[thisTotalTriCount] = new(vertices[(int)i6].Position, vertices[(int)i5].Position, vertices[(int)i4].Position);
                        thisTotalTriCount++;
                    }
                    thisTriBuffer[thisTotalTriCount] = new(vertices[(int)i1].Position, vertices[(int)i2].Position, vertices[(int)i3].Position);
                    thisTotalTriCount++;
                }

                if (isOpaque) {
                    // Good enough (I hope), opaque objects won't need it anyways (I hope).
                    var pos = vertices[(int)indices[0]].Position;
                    meshes.Add(new(new CBMesh<RMeshShader.Vertex>(gfxRes.GraphicsDevice, vertices, indices), mat, pos, isOpaque));
                } else {
                    meshes.AddRange(Helpers.SeparateVerticesIntoContinuousMeshes<RMeshShader.Vertex>(vertices, indices, x => x.Position)
                        .Select(x => new RoomData.MeshInfo(new CBMesh<RMeshShader.Vertex>(gfxRes.GraphicsDevice,
                            x.Vertices, x.Indices), mat, x.Position, isOpaque)));
                }
            }

            Span<Vector3> invisVertexStackBuffer = stackalloc Vector3[512];
            var invisVertexHeapBuffer = Array.Empty<Vector3>();

            var invisMeshCount = reader.ReadInt32();
            physics.BufferPool.ResizeToAtLeast(ref invisTriBuffer, totalInvisTriCount + meshCount * 100, totalInvisTriCount);
            for (var i = 0; i < invisMeshCount; i++) {
                var vertexCount = reader.ReadInt32();
                var vertices = GetBufferedSpan(vertexCount, invisVertexStackBuffer, ref invisVertexHeapBuffer);
                for (var j = 0; j < vertexCount; j++) {
                    vertices[j] = reader.ReadVector3() * ROOM_SCALE;
                }
                
                var triCount = reader.ReadInt32();
                physics.BufferPool.ResizeToAtLeast(ref invisTriBuffer, totalInvisTriCount + triCount * 2, totalInvisTriCount);
                for (var j = 0; j < triCount; j++) {
                    var i1 = reader.ReadInt32();
                    var i2 = reader.ReadInt32();
                    var i3 = reader.ReadInt32();
                    invisTriBuffer[totalInvisTriCount] = new(vertices[i1], vertices[i2], vertices[i3]);
                    totalInvisTriCount++;
                    invisTriBuffer[totalInvisTriCount] = new(vertices[i1], vertices[i3], vertices[i2]);
                    totalInvisTriCount++;
                }
            }

            if (hasTriggerBox) {
                var triggerBoxCount = reader.ReadInt32();
                for (var i = 0; i < triggerBoxCount; i++) {
                    var vertexCount = reader.ReadInt32();
                    var vertices = GetBufferedSpan(vertexCount, invisVertexStackBuffer, ref invisVertexHeapBuffer);
                    for (var j = 0; j < vertexCount; j++) {
                        vertices[j] = reader.ReadVector3();
                    }

                    var triCount = reader.ReadInt32();
                    for (var j = 0; j < triCount; j++) {
                        // TODO
                        reader.ReadInt32();
                        reader.ReadInt32();
                        reader.ReadInt32();
                    }

                    var name = reader.ReadB3DString();
                }
            }

            var entityCount = reader.ReadInt32();
            var entities = new List<IMapEntityData>(entityCount);
            for (var i = 0; i < entityCount; i++) {
                var typeName = reader.ReadB3DString();
                float[] angles;
                var position = Vector3.Zero;
                // The pinnacle of the rmesh spec.
                if (typeName != "model") {
                    position = reader.ReadVector3() * ROOM_SCALE;
                    // I have no idea why, but we have to flip the Z here!
                    position.Z = -position.Z;
                }
                switch (typeName) {
                    case "screen":
                        var imgpath = reader.ReadB3DString();
                        if (position != Vector3.Zero) {
                            //dic.Add("position", position);
                            //dic.Add("imgpath", imgpath);
                            //entities.Add(new(null, dic));
                        }
                        break;

                    case "waypoint":
                        //dic.Add("position", position);
                        //entities.Add(new(null, dic));
                        break;

                    case "light":
                        if (AddBasicLightInfo()) {
                            //entities.Add(new(null, dic));
                        }
                        break;

                    case "spotlight":
                        var shouldAdd = AddBasicLightInfo();
                        angles = reader.ReadB3DString()
                            .Split(' ')
                            .Select(x => float.TryParse(x, out var y)
                                ? y
                                : throw new ArgumentException("Invalid spotlight angle value"))
                            .ToArray();
                        if (angles.Length != 2 && angles.Length != 3) {
                            throw new ArgumentException("Invalid spotlight angles!");
                        }

                        var innerConeAngle = reader.ReadInt32();
                        var outerConeAngle = reader.ReadInt32();

                        if (shouldAdd) {
                            //dic.Add("rotation", Quaternion.CreateFromYawPitchRoll(angles[1], angles[0], angles.ElementAtOrDefault(2)));
                            //dic.Add("innerConeAngle", innerConeAngle);
                            //dic.Add("outerConeAngle", outerConeAngle);
                            //entities.Add(new(null, dic));
                        }
                        break;

                    case "soundemitter":
                        var sound = reader.ReadInt32();
                        var range = reader.ReadSingle();
                        //dic.Add("position", position);
                        //dic.Add("sound", sound);
                        //dic.Add("range", range);
                        //entities.Add(new(null, dic));
                        break;

                    case "playerstart":
                        angles = reader.ReadB3DString()
                            .Split(' ')
                            .Select(x =>
                                float.TryParse(x, out var y)
                                    ? y
                                    : throw new ArgumentException("Invalid playerstart rotation!"))
                            .ToArray();
                        if (angles.Length != 3) {
                            throw new ArgumentException("Invalid playerstart angles!");
                        }

                        //dic.Add("position", position);
                        //dic.Add("rotation", Quaternion.CreateFromYawPitchRoll(angles[1], angles[0], angles[2]));
                        //entities.Add(new(null, dic));
                        break;

                    case "model":
                        var file = reader.ReadB3DString();
                        position = reader.ReadVector3() * ROOM_SCALE;

                        var pitch = reader.ReadSingle();
                        var yaw = reader.ReadSingle();
                        var roll = reader.ReadSingle();

                        var scale = reader.ReadVector3() * 10f * ROOM_SCALE;

                        if (file != "") {
                            var data = new MapEntityData<Prop>(globals);
                            data.AddData("file", file);
                            data.AddData("position", position);
                            data.AddData("rotation", Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll));
                            data.AddData("scale", scale);
                            entities.Add(data);
                        }
                        break;
                }

                bool AddBasicLightInfo() {
                    var range = reader.ReadSingle() / 2000f;
                    var color = reader.ReadB3DString()
                        .Split(' ')
                        .Select(x => int.TryParse(x, out var y)
                            ? y / 255f
                            : throw new ArgumentException("Invalid light color argument!"))
                        .ToArray();
                    if (color.Length != 3) {
                        throw new ArgumentException("Invalid light color!");
                    }
                    var intensity = MathF.Min(reader.ReadSingle() * 0.8f, 1);

                    if (position != Vector3.Zero) {
                        var data = new MapEntityData<Light>(globals);
                        data.AddData("position", position);
                        data.AddData("range", range);
                        data.AddData("color", intensity * new Vector3(color[0], color[1], color[2]));
                        entities.Add(data);
                        return true;
                    }

                    return false;
                }
            }

            Mesh? visible = totalTriCount > 0
                ? Mesh.CreateWithSweepBuild(triBuffer[..totalTriCount], Vector3.One, physics.BufferPool)
                : null;
            Mesh? invisible = totalInvisTriCount > 0
                ? Mesh.CreateWithSweepBuild(invisTriBuffer[..totalInvisTriCount], Vector3.One, physics.BufferPool)
                : null;

            return new RoomData(gfxRes, physics, meshes.ToArray(), visible, invisible, entities.ToArray());
        } catch {
            physics.BufferPool.Return(ref triBuffer);
            throw;
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
