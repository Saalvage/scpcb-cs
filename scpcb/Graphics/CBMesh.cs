using Assimp;
using Veldrid;

namespace scpcb;

public interface ICBMesh {
    public void Render(CommandList commands);
}

public class CBMesh<TVertex> : Disposable, ICBMesh where TVertex : unmanaged {
    private readonly ICBMaterial<TVertex> _mat;
    private readonly DeviceBuffer _vertexBuffer;
    private readonly DeviceBuffer _indexBuffer;

    public CBMesh(GraphicsDevice gfx, ICBMaterial<TVertex> mat, TVertex[] vertices, ushort[] indices) {
        _mat = mat;
        
        _vertexBuffer = gfx.ResourceFactory.CreateVertexBuffer<TVertex>((uint)vertices.Length);
        gfx.UpdateBuffer(_vertexBuffer, 0, vertices);

        _indexBuffer = gfx.ResourceFactory.CreateBuffer(new((uint)indices.Length * sizeof(ushort), BufferUsage.IndexBuffer));
        gfx.UpdateBuffer(_indexBuffer, 0, indices);
    }

    public CBMesh(GraphicsDevice gfx, ICBMaterial<TVertex> mat, string file) {
        _mat = mat;

        // Load model using assimp
        var importer = new AssimpContext();
        var scene = importer.ImportFile(file, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs);
        
    }

    public void Render(CommandList commands) {
        _mat.Apply(commands);
        commands.SetVertexBuffer(0, _vertexBuffer);
        commands.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
        commands.DrawIndexed(6, 1, 0, 0, 0);
    }

    protected override void DisposeImpl() {
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
    }
}
