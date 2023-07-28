using Veldrid;

namespace scpcb.Graphics;

public interface ICBMesh {
    public ICBMaterial Material { get; }

    public void Render(CommandList commands);
}

public interface ICBMesh<TVertex> : ICBMesh where TVertex : unmanaged {
    public ICBMaterial<TVertex> Material { get; }

    ICBMaterial ICBMesh.Material => Material;
}

public class CBMesh<TVertex> : Disposable, ICBMesh<TVertex> where TVertex : unmanaged {
    public ICBMaterial<TVertex> Material { get; }

    private readonly DeviceBuffer _vertexBuffer;
    private readonly DeviceBuffer _indexBuffer;
    private readonly uint _indexCount;

    public CBMesh(GraphicsDevice gfx, ICBMaterial<TVertex> mat, ReadOnlySpan<TVertex> vertices, ReadOnlySpan<uint> indices) {
        Material = mat;
        
        _vertexBuffer = gfx.ResourceFactory.CreateVertexBuffer<TVertex>((uint)vertices.Length);
        gfx.UpdateBuffer(_vertexBuffer, 0, vertices);

        _indexCount = (uint)indices.Length;
        _indexBuffer = gfx.ResourceFactory.CreateBuffer(new(_indexCount * sizeof(uint), BufferUsage.IndexBuffer));
        gfx.UpdateBuffer(_indexBuffer, 0, indices);
    }

    public void Render(CommandList commands) {
        Material.Apply(commands);
        commands.SetVertexBuffer(0, _vertexBuffer);
        commands.SetIndexBuffer(_indexBuffer, IndexFormat.UInt32);
        commands.DrawIndexed(_indexCount, 1, 0, 0, 0);
    }

    protected override void DisposeImpl() {
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
    }
}
