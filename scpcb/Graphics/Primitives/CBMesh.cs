using scpcb.Graphics.Shaders.Utility;
using scpcb.Utility;
using Veldrid;

namespace scpcb.Graphics.Primitives;

public interface ICBMesh : IDisposable {
    void ApplyGeometry(CommandList commands);
    void Draw(CommandList commands);

    ICBModel CreateModel(ICBMaterial mat, IConstantHolder? constants);
}

public interface ICBMesh<TVertex> : ICBMesh where TVertex : unmanaged {
    ICBModel ICBMesh.CreateModel(ICBMaterial mat, IConstantHolder? constants) {
        if (mat is not ICBMaterial<TVertex> vMat) {
            throw new ArgumentException($"Material must have vertex type {typeof(TVertex)}", nameof(mat));
        }
        return CreateModel(vMat, constants);
    }

    ICBModel<TVertex> CreateModel(ICBMaterial<TVertex> mat, IConstantHolder? constants);
}

public class CBMesh<TVertex> : Disposable, ICBMesh<TVertex> where TVertex : unmanaged {
    private readonly DeviceBuffer _vertexBuffer;
    private readonly DeviceBuffer _indexBuffer;
    private readonly uint _indexCount;

    public CBMesh(GraphicsDevice gfx, ReadOnlySpan<TVertex> vertices, ReadOnlySpan<uint> indices) {
        _vertexBuffer = gfx.ResourceFactory.CreateVertexBuffer<TVertex>((uint)vertices.Length);
        gfx.UpdateBuffer(_vertexBuffer, 0, vertices);

        _indexCount = (uint)indices.Length;
        _indexBuffer = gfx.ResourceFactory.CreateBuffer(new(_indexCount * sizeof(uint), BufferUsage.IndexBuffer));
        gfx.UpdateBuffer(_indexBuffer, 0, indices);
    }

    public void ApplyGeometry(CommandList commands) {
        commands.SetVertexBuffer(0, _vertexBuffer);
        commands.SetIndexBuffer(_indexBuffer, IndexFormat.UInt32);
    }

    public void Draw(CommandList commands) {
        commands.DrawIndexed(_indexCount, 1, 0, 0, 0);
    }

    public ICBModel<TVertex> CreateModel(ICBMaterial<TVertex> mat, IConstantHolder constants)
        => new CBModel<TVertex>(constants, mat, this);

    protected override void DisposeImpl() {
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
    }
}
