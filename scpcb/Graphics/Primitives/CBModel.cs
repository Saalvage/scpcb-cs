using scpcb.Graphics.Shaders.Utility;

namespace scpcb.Graphics.Primitives;

// TODO: Consider making this mutable?
public interface ICBModel {
    IConstantHolder? Constants { get; }

    ICBMaterial Material { get; }
    ICBMesh Mesh { get; }

    List<IConstantProvider> ConstantProviders { get; }

    bool IsVisible { get; set; }
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
}
