using System.Numerics;
using Veldrid;

namespace scpcb.Shaders; 

public class ModelShader : CBShader<ModelShader.Vertex, ModelShader.VertUniforms, ModelShader.FragUniforms> {
    public record struct Vertex(Vector3 Position, Vector2 Uv);
    public record struct VertUniforms(Matrix4x4 Projection, Matrix4x4 View, Matrix4x4 Model);
    public record struct FragUniforms;

    public ModelShader(GraphicsDevice gfx) : base(gfx, @"
#version 450

layout(set = 0, binding = 0) uniform Idfdkk {
    mat4 Projection;
    mat4 View;
    mat4 Model;
} constants;

layout(location = 0) in vec3 Position;
layout(location = 1) in vec2 Uv;

layout(location = 0) out vec2 fsin_Uv;

void main() {
    gl_Position = constants.Projection * constants.View * constants.Model * vec4(Position, 1);
    fsin_Uv = Uv;
}",
        @"
#version 450

layout(location = 0) in vec2 fsin_Uv;

layout(location = 0) out vec4 fsout_Color;

layout(set = 1, binding = 0) uniform texture2D texture0;
layout(set = 1, binding = 1) uniform sampler samper;

void main() {
    fsout_Color = texture(sampler2D(texture0, samper), fsin_Uv);
}", 1) {

    }
}
