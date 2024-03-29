using scpcb.Graphics.Shaders.Utility;
using scpcb.Graphics.Textures;

namespace scpcb.Graphics.Primitives;

// TODO: Consider making this mutable?
public interface ICBModel {
    IConstantHolder? Constants { get; }

    ICBMaterial Material { get; }
    ICBMesh Mesh { get; }

    List<IConstantProvider> ConstantProviders { get; }

    bool IsVisible { get; set; }

    void Render(IRenderTarget target, float interp);
}

public interface ICBModel<TVertex> : ICBModel where TVertex : unmanaged {
    new ICBMaterial<TVertex> Material { get; }
    ICBMaterial ICBModel.Material => Material;

    new ICBMesh<TVertex> Mesh { get; }
    ICBMesh ICBModel.Mesh => Mesh;
}

public interface ICBModel<TVertConstants, TFragConstants> : ICBModel
        where TVertConstants : unmanaged where TFragConstants : unmanaged {
    new IConstantHolder<TVertConstants, TFragConstants>? Constants { get; }
    IConstantHolder? ICBModel.Constants => Constants;
}

public record CBModel<TVertex>(IConstantHolder? Constants, ICBMaterial<TVertex> Material, ICBMesh<TVertex> Mesh)
        : ICBModel<TVertex> where TVertex : unmanaged {
    public List<IConstantProvider> ConstantProviders { get; } = [];
    public bool IsVisible { get; set; } = true;

    public void Render(IRenderTarget target, float interp) {
        if (!IsVisible) {
            return;
        }

        // TODO: Revisit this. Is there a better design?
        // At the very least some more caching can be done.
        foreach (var cp in ConstantProviders) {
            cp.ApplyTo([Material.Shader.Constants, Constants], interp);
        }

        target.Render(new MeshMaterial<TVertex>(Mesh, Material), Constants);
    }
}
