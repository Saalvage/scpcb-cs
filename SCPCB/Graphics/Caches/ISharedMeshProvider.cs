﻿using SCPCB.Graphics.Primitives;

namespace SCPCB.Graphics.Caches; 

public interface ISharedMeshProvider<TSelf> where TSelf : ISharedMeshProvider<TSelf> {
    static abstract ICBMesh CreateSharedMesh(GraphicsResources gfxRes);
}

public interface ISharedMeshProvider<TSelf, TVertex> : ISharedMeshProvider<TSelf> where TVertex : unmanaged where TSelf : ISharedMeshProvider<TSelf, TVertex> {
    static ICBMesh ISharedMeshProvider<TSelf>.CreateSharedMesh(GraphicsResources gfxRes) => TSelf.CreateSharedMesh(gfxRes);
    new static abstract ICBMesh<TVertex> CreateSharedMesh(GraphicsResources gfxRes);
}
