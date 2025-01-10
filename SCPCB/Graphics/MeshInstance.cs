using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders.Utility;
using SCPCB.Graphics.Textures;

namespace SCPCB.Graphics;

// TODO: Consider making this mutable?
public interface IMeshInstance {
    IConstantHolder? Constants { get; }

    ICBMaterial Material { get; }
    ICBMesh Mesh { get; }

    List<IConstantProvider> ConstantProviders { get; }

    bool IsVisible { get; set; }

    void Render(IRenderTarget target, float interp);
}

public interface IMeshInstance<TVertex> : IMeshInstance where TVertex : unmanaged {
    new ICBMaterial<TVertex> Material { get; }
    ICBMaterial IMeshInstance.Material => Material;

    new ICBMesh<TVertex> Mesh { get; }
    ICBMesh IMeshInstance.Mesh => Mesh;
}

public interface IMeshInstance<TVertConstants, TFragConstants> : IMeshInstance
        where TVertConstants : unmanaged where TFragConstants : unmanaged {
    new IConstantHolder<TVertConstants, TFragConstants>? Constants { get; }
    IConstantHolder? IMeshInstance.Constants => Constants;
}

// TODO: We should assert that the constants have the correct layout.
public class MeshInstance<TVertex> : IMeshInstance<TVertex> where TVertex : unmanaged {
    public IConstantHolder? Constants { get; }
    public ICBMaterial<TVertex> Material { get; }
    public ICBMesh<TVertex> Mesh { get; }

    public List<IConstantProvider> ConstantProviders { get; } = [];
    public bool IsVisible { get; set; } = true;

    public MeshInstance(IConstantHolder? constants, ICBMaterial<TVertex> material, ICBMesh<TVertex> mesh) {
        Constants = constants;
        Material = material;
        Mesh = mesh;
    }

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
