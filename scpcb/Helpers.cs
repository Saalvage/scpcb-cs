using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace scpcb; 

public static class Helpers {
    public readonly record struct TypeInfo(VertexElementFormat Format, string Name);

    private static TypeInfo TypeToFormat(Type type) {
        if (type == typeof(float)) {
            return new(VertexElementFormat.Float1, "float");
        } else if (type == typeof(Vector2)) {
            return new(VertexElementFormat.Float2, "vec2");
        } else if (type == typeof(Vector3)) {
            return new(VertexElementFormat.Float3, "vec3");
        } else if (type == typeof(Vector4)) {
            return new(VertexElementFormat.Float4, "vec4");
        } else if (type == typeof(int)) {
            return new(VertexElementFormat.Int1, "int");
        } else if (type == typeof(uint)) {
            return new(VertexElementFormat.UInt1, "uint");
        } else if (type == typeof(RgbaFloat)) {
            return new(VertexElementFormat.Float4, "vec4");
        }

        throw new NotImplementedException();
    }

    public static unsafe VertexLayoutDescription GetDescriptionFromType<T>() where T : unmanaged {
        var properties = typeof(T).GetProperties();
        if (properties.Sum(x => Marshal.SizeOf(x.PropertyType)) != sizeof(T)) {
            throw new InvalidOperationException("Size of struct does not equal sum of properties");
        }
        return new(properties
            .Select(x =>
                new VertexElementDescription(x.Name, TypeToFormat(x.PropertyType).Format,
                    VertexElementSemantic.TextureCoordinate))
            .ToArray());
    }

    public static unsafe DeviceBuffer CreateVertexBuffer<T>(this ResourceFactory factory, uint count) where T : unmanaged
        => factory.CreateBuffer(new(count * (uint)sizeof(T), BufferUsage.VertexBuffer));
}
