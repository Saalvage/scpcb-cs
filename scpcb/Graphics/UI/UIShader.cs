using System.Numerics;
using Veldrid;

namespace scpcb.Graphics.UI;

public class UIShader : CBShader<UIShader.Vertex, UIShader.VertUniforms, UIShader.FragUniforms> {
    public record struct Vertex(Vector2 Position, Vector2 Uv);

    public UIShader(GraphicsDevice gfx) : base(gfx, @"
#version 450

layout(set = 0, binding = 0) uniform Idfdkk {
    mat4 Projection;
    vec2 Position;
    vec2 Scale;
} constants;

layout(location = 0) in vec2 Position;
layout(location = 1) in vec2 Uv;

layout(location = 0) out vec2 fsin_Uv;

void main() {
    gl_Position = constants.Projection * vec4(Position * constants.Scale + constants.Position, -1, 1);
    fsin_Uv = Uv;
}"u8.ToArray(),
        @"
#version 450

layout(location = 0) in vec2 fsin_Uv;

layout(location = 0) out vec4 fsout_Color;

layout(set = 1, binding = 0) uniform texture2D texture0;
layout(set = 1, binding = 1) uniform sampler samper;

void main() {
    fsout_Color = texture(sampler2D(texture0, samper), fsin_Uv);
}"u8.ToArray(), 1) {
        VertexConstants.Scale = Vector2.One;
    }

    public record struct VertUniforms(Matrix4x4 Projection, Vector2 Position, Vector2 Scale);
    public record struct FragUniforms; 
}
