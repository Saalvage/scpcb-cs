using Veldrid;

namespace scpcb.Graphics;

public interface ICBMesh : IDisposable {
    public void ApplyGeometry(CommandList commands);
    public void Draw(CommandList commands);
}

public interface ICBMesh<TVertex> : ICBMesh where TVertex : unmanaged { }

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

    protected override void DisposeImpl() {
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
    }
}
