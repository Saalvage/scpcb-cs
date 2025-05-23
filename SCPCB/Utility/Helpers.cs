﻿using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using SCPCB.Map;
using ShaderGen;
using Veldrid;

namespace SCPCB.Utility; 

public static class Helpers {
    /// <summary>
    /// FPSfactor is commonly used in various formulas within CB.
    /// </summary>
    /// <para>Our delta is in total seconds, FPSFactor is in seconds * 70 (for whatever reason).</para>
    public const float DELTA_TO_FPS_FACTOR_FACTOR = 70f;

    private static VertexElementFormat TypeToFormat(Type type) {
        if (type == typeof(float)) {
            return VertexElementFormat.Float1;
        } else if (type == typeof(Vector2)) {
            return VertexElementFormat.Float2;
        } else if (type == typeof(Vector3)) {
            return VertexElementFormat.Float3;
        } else if (type == typeof(Vector4)) {
            return VertexElementFormat.Float4;
        } else if (type == typeof(RgbaFloat)) {
            return VertexElementFormat.Float4;
        } else if (type == typeof(int)) {
            return VertexElementFormat.Int1;
        } else if (type == typeof(Int2)) {
            return VertexElementFormat.Int2;
        } else if (type == typeof(Int3)) {
            return VertexElementFormat.Int3;
        } else if (type == typeof(Int4)) {
            return VertexElementFormat.Int4;
        } else if (type == typeof(uint)) {
            return VertexElementFormat.UInt1;
        } else if (type == typeof(UInt2)) {
            return VertexElementFormat.UInt2;
        } else if (type == typeof(UInt3)) {
            return VertexElementFormat.UInt3;
        } else if (type == typeof(UInt4)) {
            return VertexElementFormat.UInt4;
        }

        throw new NotImplementedException();
    }

    private static VertexElementSemantic MemberToSemantic(MemberInfo property) {
        var attr = property.GetCustomAttribute<VertexSemanticAttribute>();
        if (attr != null) {
            // TODO: This is fine for now, but why are they separate?
            return (VertexElementSemantic)attr.Type-1;
        }

        return (VertexElementSemantic)(property.DeclaringType.GetConstructors().Single().GetParameters()
            .First(x => x.Name == property.Name).GetCustomAttribute<VertexSemanticAttribute>().Type-1);
    }

    public static unsafe VertexLayoutDescription GetDescriptionFromType<T>() where T : unmanaged {
        var properties = GetFieldsAndProperties<T>().ToArray();
        if (properties.Sum(x => Marshal.SizeOf(x.Type)) != sizeof(T)) {
            throw new InvalidOperationException("Size of struct does not equal sum of properties");
        }
        return new(properties
            .Select(x => new VertexElementDescription(x.Member.Name, TypeToFormat(x.Type), MemberToSemantic(x.Member)))
            // If this continues causing issues, look into implementing the offset.
            .ToArray());
    }

    public static IEnumerable<(MemberInfo Member, Type Type)> GetFieldsAndProperties<T>() => GetFieldsAndProperties(typeof(T));

    public static IEnumerable<(MemberInfo Member, Type Type)> GetFieldsAndProperties(this Type type)
        => type.GetMembers() // We're doing it like this to get them in the correct order.
            .Where(x => x is PropertyInfo or FieldInfo)
            .Select(x => (x, MemberToType(x)));

    private static Type MemberToType(MemberInfo info) => info switch {
        PropertyInfo prop => prop.PropertyType,
        FieldInfo field => field.FieldType,
    };

    public static unsafe DeviceBuffer CreateVertexBuffer<T>(this ResourceFactory factory, uint count) where T : unmanaged
        => factory.CreateBuffer(new(count * (uint)sizeof(T), BufferUsage.VertexBuffer));

    public static (TVertex[] Vertices, uint[] Indices, Vector3 Position)[] SeparateVerticesIntoContinuousMeshes<TVertex>(
            ReadOnlySpan<TVertex> vertices, ReadOnlySpan<uint> indices, Func<TVertex, Vector3> positionFunc)
                where TVertex : IEquatable<TVertex> {

        // TODO: This can probably be optimized further.
        // At the very least by reducing heap allocs by a lot and dealing with the reverse tris explicitly.
        // But does it really matter?

        // 1. Remove all duplicate vertices (yes, there are duplicate vertices)
        // 2. Separate mesh into parts that are not connected.
        // 3. Determine vertices per mesh.

        // 1.
        var remainingIndices = new List<uint>();
        remainingIndices.AddRange(indices);
        ReplaceIndices(remainingIndices, DeduplicateIndices(vertices));

        // 2.
        var indicesPerMesh = new List<List<uint>>();

        while (remainingIndices.Count != 0) {
            var currList = new List<uint>();
            // Begin from the end since erasure there is faster.
            currList.Add(remainingIndices[^3]);
            currList.Add(remainingIndices[^2]);
            currList.Add(remainingIndices[^1]);
            remainingIndices.RemoveRange(remainingIndices.Count - 3, 3);
            var addedAny = true;
            while (addedAny) {
                addedAny = false;
                for (var j = 0; j < remainingIndices.Count; j += 3) {
                    if (currList.Contains(remainingIndices[j])
                        || currList.Contains(remainingIndices[j + 1])
                        || currList.Contains(remainingIndices[j + 2])) {
                        currList.Add(remainingIndices[j + 0]);
                        currList.Add(remainingIndices[j + 1]);
                        currList.Add(remainingIndices[j + 2]);
                        remainingIndices.RemoveRange(j, 3);
                        addedAny = true;
                        break;
                    }
                }
            }
            indicesPerMesh.Add(currList);
        }

        // 3.
        var meshVertices = new List<TVertex>();
        var indicesMapper = new Dictionary<uint, uint>();
        var ret = new (TVertex[] Vertices, uint[] Indices, Vector3 Position)[indicesPerMesh.Count];
        for (var i = 0; i < indicesPerMesh.Count; i++) {
            var meshIndices = indicesPerMesh[i];
            meshVertices.Clear();
            indicesMapper.Clear();
            var vertexSum = Vector3.Zero;
            for (var j = 0; j < meshIndices.Count; j++) {
                var oldIndex = meshIndices[j];
                if (!indicesMapper.TryGetValue(oldIndex, out var newIndex)) {
                    newIndex = (uint)meshVertices.Count;
                    indicesMapper.Add(oldIndex, newIndex);
                    meshVertices.Add(vertices[(int)oldIndex]);
                    vertexSum += positionFunc(meshVertices.Last());
                }

                meshIndices[j] = newIndex;
            }

            ret[i] = (meshVertices.ToArray(), meshIndices.ToArray(), vertexSum / meshVertices.Count);
        }

        return ret;

        static int[] DeduplicateIndices(ReadOnlySpan<TVertex> vertices) {
            var meshVertices = Enumerable.Range(0, vertices.Length).ToArray();
            for (var i = 0; i < vertices.Length; i++) {
                for (var j = i + 1; j < vertices.Length; j++) {
                    if (vertices[i].Equals(vertices[j])) {
                        meshVertices[j] = i;
                    }
                }
            }

            return meshVertices;
        }

        static void ReplaceIndices(IList<uint> sourceIndices, int[] replaceWithIndices) {
            for (var i = 0; i < sourceIndices.Count; i++) {
                sourceIndices[i] = (uint)replaceWithIndices[sourceIndices[i]];
            }
        }
    }

    public static IEnumerable<Type> GetAllLoadedTypes()
        => AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.DefinedTypes);

    public static Color ColorFromHSV(double hue, double saturation, double value) {
        var hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        var f = hue / 60 - Math.Floor(hue / 60);

        value *= 255;
        var v = Convert.ToInt32(value);
        var p = Convert.ToInt32(value * (1 - saturation));
        var q = Convert.ToInt32(value * (1 - f * saturation));
        var t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

        return hi switch {
            0 => Color.FromArgb(255, v, t, p),
            1 => Color.FromArgb(255, q, v, p),
            2 => Color.FromArgb(255, p, v, t),
            3 => Color.FromArgb(255, p, q, v),
            4 => Color.FromArgb(255, t, p, v),
            _ => Color.FromArgb(255, v, p, q),
        };
    }

    public static Matrix4x4 CreateUIProjectionMatrix(int width, int height, int zNear = -100, int zFar = 100) => new Matrix4x4(
        1, 0, 0, 0,
        0, -1, 0, 0,
        0, 0, 1, 0,
        -width / 2f, height / 2f, 0, 1
    ) * Matrix4x4.CreateOrthographic(width, height, zNear, zFar);

    public static uint RoundUpToMultiple(uint value, uint multipleOf) {
        return (value + multipleOf - 1) / multipleOf * multipleOf;
    }

    public static Vector3 ComputeNormal(Vector3 a, Vector3 b, Vector3 c) => Vector3.Normalize(Vector3.Cross(b - a, c - a));

    // https://stackoverflow.com/a/52551983/15262536
    public static Quaternion CreateLookAtQuaternion(Vector3 forward, Vector3 up) {
        var F = Vector3.Normalize(forward);
        var R = Vector3.Normalize(Vector3.Cross(up, F));
        var U = Vector3.Cross(F, R);

        Quaternion q;
        var trace = R.X + U.Y + F.Z;
        if (trace > 0.0) {
            var s = 0.5f / MathF.Sqrt(trace + 1f);
            q.W = 0.25f / s;
            q.X = (U.Z - F.Y) * s;
            q.Y = (F.X - R.Z) * s;
            q.Z = (R.Y - U.X) * s;
        } else {
            if (R.X > U.Y && R.X > F.Z) {
                var s = 2f * MathF.Sqrt(1f + R.X - U.Y - F.Z);
                q.W = (U.Z - F.Y) / s;
                q.X = 0.25f * s;
                q.Y = (U.X + R.Y) / s;
                q.Z = (F.X + R.Z) / s;
            } else if (U.Y > F.Z) {
                var s = 2f * MathF.Sqrt(1f + U.Y - R.X - F.Z);
                q.W = (F.X - R.Z) / s;
                q.X = (U.X + R.Y) / s;
                q.Y = 0.25f * s;
                q.Z = (F.Y + U.Z) / s;
            } else {
                var s = 2f * MathF.Sqrt(1f + F.Z - R.X - U.Y);
                q.W = (R.Y - U.X) / s;
                q.X = (F.X + R.Z) / s;
                q.Y = (F.Y + U.Z) / s;
                q.Z = 0.25f * s;
            }
        }

        return q;
    }

    public static PlacedRoomInfo[,] GenerateDebugRooms() {
        var rooms = JsonSerializer.Deserialize<RoomInfo[]>(File.ReadAllText("Assets/Rooms/rooms.json"));
        var grid = new PlacedRoomInfo[5, 10];

        foreach (var i in Enumerable.Range(0, 5)) {
            foreach (var j in Enumerable.Range(0, 10)) {
                var roomName =
                    i == 0 || i == 4 || j == 0 || j == 9 ? "room008" :
                    i == 2 || j == 5 ? "coffin" : "4tunnels";
                grid[i, j] = new(rooms.First(x => x.Name == roomName), (Direction)((i + j) % 4));
            }
        }
        grid[0, 0] = new(rooms.First(x => x.Name == "test"), Direction.Up);

        return grid;
    }
}
