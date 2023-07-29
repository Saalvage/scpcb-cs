using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using ShaderGen;
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

    private static VertexElementSemantic PropertyToSemantic(PropertyInfo property) {
        var attr = property.GetCustomAttribute<VertexSemanticAttribute>();
        if (attr != null) {
            // TODO: This is fine for now, but why are they separate?
            return (VertexElementSemantic)attr.Type-1;
        }

        return (VertexElementSemantic)(property.DeclaringType.GetConstructors().Single().GetParameters()
            .First(x => x.Name == property.Name).GetCustomAttribute<VertexSemanticAttribute>().Type-1);
    }

    public static unsafe VertexLayoutDescription GetDescriptionFromType<T>() where T : unmanaged {
        var properties = typeof(T).GetProperties();
        if (properties.Sum(x => Marshal.SizeOf(x.PropertyType)) != sizeof(T)) {
            throw new InvalidOperationException("Size of struct does not equal sum of properties");
        }
        return new(properties
            .Select(x => new VertexElementDescription(x.Name, TypeToFormat(x.PropertyType).Format, PropertyToSemantic(x)))
            // If this continues causing issues, look into implementing the offset
            .ToArray());
    }

    public static unsafe DeviceBuffer CreateVertexBuffer<T>(this ResourceFactory factory, uint count) where T : unmanaged
        => factory.CreateBuffer(new(count * (uint)sizeof(T), BufferUsage.VertexBuffer));

    public static bool EqualFloats(float a, float b, float eps = 0.001f)
        => Math.Abs(a - b) < eps;

    public static Plane PlaneFromNormalAndPoint(Vector3 normal, Vector3 pointOnPlane) {
        var normalizedNormal = normal.SafeNormalize();
        return new(normalizedNormal, Vector3.Dot(normalizedNormal, pointOnPlane));
    }

    public static Plane PlaneFromPoints(Vector3 p1, Vector3 p2, Vector3 p3)
        => PlaneFromNormalAndPoint(Vector3.Cross((p3 - p1), (p2 - p1)), p1);
}
