﻿using scpcb.Graphics.Shaders;

namespace scpcb.Graphics.Assimp;

/// <summary>
/// Because <see cref="AssimpVertex"/> is a ref struct, we can't use it as a generic type parameter to <see cref="IVertexConvertible{TTo,TFrom}"/>.
/// </summary>
/// <typeparam name="TOut"></typeparam>
public interface IAssimpVertexConvertible<TOut> where TOut : unmanaged {
    public static abstract TOut ConvertVertex(AssimpVertex vert);
}