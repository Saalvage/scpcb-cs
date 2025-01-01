using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Utility;
using Veldrid;

namespace SCPCB.Graphics.Shaders.Utility;

public interface IConstantHolder : IDisposable {
    bool HasConstant<T>() where T : IConstantMember<T>;
    void SetValue<T, TVal>(TVal val) where T : IConstantMember<T, TVal> where TVal : unmanaged, IEquatable<TVal>;
    void UpdateAndSetBuffers(CommandList commands, uint index);
}

public interface IConstantHolder<TVertConstants, TFragConstants> : IConstantHolder
        where TVertConstants : unmanaged where TFragConstants : unmanaged { }

public class ConstantHolder<TVertConstants, TFragConstants> : Disposable, IConstantHolder<TVertConstants, TFragConstants>
        where TVertConstants : unmanaged where TFragConstants : unmanaged {
    private TVertConstants _lastVert;
    private readonly object _vertexBoxed = default(TVertConstants);
    private ref TVertConstants _vert => ref Unsafe.Unbox<TVertConstants>(_vertexBoxed);

    private TFragConstants _lastFrag;
    private readonly object _fragmentBoxed = default(TFragConstants);
    private ref TFragConstants _frag => ref Unsafe.Unbox<TFragConstants>(_fragmentBoxed);

    private readonly DeviceBuffer? _vertexBuffer;
    private readonly DeviceBuffer? _fragmentBuffer;
    private readonly ResourceSet _set;

    private bool _hasEverUpdated = false;
    private bool _isDirty = true; // Initially true, because we've never updated!

    public static ResourceLayout? TryCreateLayout(GraphicsDevice gfx, string? vertConstantNames, string? fragConstantNames) {
        var hasVertConsts = typeof(TVertConstants) != typeof(Empty);
        var hasFragConsts = typeof(TFragConstants) != typeof(Empty);
        if (!hasVertConsts && !hasFragConsts) {
            return null;
        }

        var layouts = new List<ResourceLayoutElementDescription>();
        if (hasVertConsts) {
            layouts.Add(new(vertConstantNames, ResourceKind.UniformBuffer, ShaderStages.Vertex));
        }

        if (hasFragConsts) {
            layouts.Add(new(fragConstantNames, ResourceKind.UniformBuffer, ShaderStages.Fragment));
        }

        return gfx.ResourceFactory.CreateResourceLayout(new(layouts.ToArray()));
    }

    public static ConstantHolder<TVertConstants, TFragConstants>? TryCreate(GraphicsDevice gfx, ResourceLayout? layout) {
        var hasVertConsts = typeof(TVertConstants) != typeof(Empty);
        var hasFragConsts = typeof(TFragConstants) != typeof(Empty);
        if (!hasVertConsts && !hasFragConsts) {
            return null;
        }

        return new(gfx, layout);
    }

    public unsafe ConstantHolder(GraphicsDevice gfx, ResourceLayout layout) {
        var hasVertConsts = typeof(TVertConstants) != typeof(Empty);
        var hasFragConsts = typeof(TFragConstants) != typeof(Empty);
        if (!hasVertConsts && !hasFragConsts) {
            throw new InvalidOperationException("Constant holder without constants to hold!");
        }

        var consts = new List<BindableResource>();
        if (hasVertConsts) {
            _vertexBuffer = CreateBuffer<TVertConstants>();
            consts.Add(_vertexBuffer);
        }
        if (hasFragConsts) {
            _fragmentBuffer = CreateBuffer<TFragConstants>();
            consts.Add(_fragmentBuffer);
        }

        _set = gfx.ResourceFactory.CreateResourceSet(new(layout, consts.ToArray()));

        DeviceBuffer CreateBuffer<T>() where T : unmanaged {
            var propertySize = typeof(T).GetProperties().Sum(x => Marshal.SizeOf(x.PropertyType));
            if (propertySize != 0 && propertySize != sizeof(T)) { // TODO: Deal with 0
                throw new InvalidOperationException("Size of struct does not equal sum of properties");
            }
            return gfx.ResourceFactory.CreateBuffer(new(RoundTo32Bytes(propertySize), BufferUsage.UniformBuffer));

            static uint RoundTo32Bytes(int num) {
                uint run = 32;
                while (run < num) {
                    run += 32;
                }
                return run;
            }
        }
    }

    // Taken from SpanHelpers.SequenceEqual, the jump magic genuinely improves performance.
    // We put the small cases at the very end because small sizes are highly unusual for constant buffers.
    // We also ignore a special case for >=64 (but <=128) bits on 64 bit machines with 128 vector support.
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static unsafe bool SequenceEqual(ref byte first, ref byte second, nuint length) {
        bool result;
        // Use nint for arithmetic to avoid unnecessary 64->32->64 truncations
        if (Vector128.IsHardwareAccelerated) {
            if (Vector512.IsHardwareAccelerated && length >= (nuint)Vector512<byte>.Count) {
                nuint offset = 0;
                nuint lengthToExamine = length - (nuint)Vector512<byte>.Count;
                // Unsigned, so it shouldn't have overflowed larger than length (rather than negative)
                Debug.Assert(lengthToExamine < length);
                if (lengthToExamine != 0) {
                    do {
                        if (Vector512.LoadUnsafe(ref first, offset) !=
                            Vector512.LoadUnsafe(ref second, offset)) {
                            goto NotEqual;
                        }
                        offset += (nuint)Vector512<byte>.Count;
                    } while (lengthToExamine > offset);
                }

                // Do final compare as Vector512<byte>.Count from end rather than start
                if (Vector512.LoadUnsafe(ref first, lengthToExamine) ==
                    Vector512.LoadUnsafe(ref second, lengthToExamine)) {
                    // C# compiler inverts this test, making the outer goto the conditional jmp.
                    goto Equal;
                }

                // This becomes a conditional jmp forward to not favor it.
                goto NotEqual;
            } else if (Vector256.IsHardwareAccelerated && length >= (nuint)Vector256<byte>.Count) {
                nuint offset = 0;
                nuint lengthToExamine = length - (nuint)Vector256<byte>.Count;
                // Unsigned, so it shouldn't have overflowed larger than length (rather than negative)
                Debug.Assert(lengthToExamine < length);
                if (lengthToExamine != 0) {
                    do {
                        if (Vector256.LoadUnsafe(ref first, offset) !=
                            Vector256.LoadUnsafe(ref second, offset)) {
                            goto NotEqual;
                        }
                        offset += (nuint)Vector256<byte>.Count;
                    } while (lengthToExamine > offset);
                }

                // Do final compare as Vector256<byte>.Count from end rather than start
                if (Vector256.LoadUnsafe(ref first, lengthToExamine) ==
                    Vector256.LoadUnsafe(ref second, lengthToExamine)) {
                    // C# compiler inverts this test, making the outer goto the conditional jmp.
                    goto Equal;
                }

                // This becomes a conditional jmp forward to not favor it.
                goto NotEqual;
            } else if (length >= (nuint)Vector128<byte>.Count) {
                nuint offset = 0;
                nuint lengthToExamine = length - (nuint)Vector128<byte>.Count;
                // Unsigned, so it shouldn't have overflowed larger than length (rather than negative)
                Debug.Assert(lengthToExamine < length);
                if (lengthToExamine != 0) {
                    do {
                        if (Vector128.LoadUnsafe(ref first, offset) !=
                            Vector128.LoadUnsafe(ref second, offset)) {
                            goto NotEqual;
                        }
                        offset += (nuint)Vector128<byte>.Count;
                    } while (lengthToExamine > offset);
                }

                // Do final compare as Vector128<byte>.Count from end rather than start
                if (Vector128.LoadUnsafe(ref first, lengthToExamine) ==
                    Vector128.LoadUnsafe(ref second, lengthToExamine)) {
                    // C# compiler inverts this test, making the outer goto the conditional jmp.
                    goto Equal;
                }

                // This becomes a conditional jmp forward to not favor it.
                goto NotEqual;
            }
        }

        // We don't implement any of the special cases for small lengths as these are highly untypical
        // of constant buffers (we're talking less than a Vector4 here)
        if (length >= (nuint)sizeof(nuint)) {
            nuint offset = 0;
            nuint lengthToExamine = length - (nuint)sizeof(nuint);
            // Unsigned, so it shouldn't have overflowed larger than length (rather than negative)
            Debug.Assert(lengthToExamine < length);
            if (lengthToExamine > 0) {
                do {
                    // Compare unsigned so not do a sign extend mov on 64 bit
                    if (Unsafe.ReadUnaligned<nuint>(ref Unsafe.AddByteOffset(ref first, offset))
                        != Unsafe.ReadUnaligned<nuint>(ref Unsafe.AddByteOffset(ref second, lengthToExamine))) {
                        goto NotEqual;
                    }
                    offset += (nuint)sizeof(nuint);
                } while (lengthToExamine > offset);
            }

            // Do final compare as sizeof(nuint) from end rather than start
            result = (Unsafe.ReadUnaligned<nuint>(ref Unsafe.AddByteOffset(ref first, lengthToExamine))
                      == Unsafe.ReadUnaligned<nuint>(ref Unsafe.AddByteOffset(ref second, lengthToExamine)));
            goto Result;
        }

        if (length < sizeof(uint)) {
            uint differentBits = 0;
            nuint offset = (length & 2);
            if (offset != 0) {
                differentBits = Unsafe.ReadUnaligned<ushort>(ref first);
                differentBits -= Unsafe.ReadUnaligned<ushort>(ref second);
            }
            if ((length & 1) != 0) {
                differentBits |= (uint)Unsafe.AddByteOffset(ref first, offset) - (uint)Unsafe.AddByteOffset(ref second, offset);
            }
            result = (differentBits == 0);
            goto Result;
        } else {
            nuint offset = length - sizeof(uint);
            uint differentBits = Unsafe.ReadUnaligned<uint>(ref first) - Unsafe.ReadUnaligned<uint>(ref second);
            differentBits |= Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref first, offset)) - Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref second, offset));
            result = (differentBits == 0);
            goto Result;
        }

    Result:
        return result;
        // When the sequence is equal; which is the longest execution, we want it to determine that
        // as fast as possible so we do not want the early outs to be "predicted not taken" branches.
    Equal:
        return true;

        // As there are so many true/false exit points the Jit will coalesce them to one location.
        // We want them at the end so the conditional early exit jmps are all jmp forwards so the
        // branch predictor in a uninitialized state will not take them e.g.
        // - loops are conditional jmps backwards and predicted
        // - exceptions are conditional forwards jmps and not predicted
    NotEqual:
        return false;
    }

    public void UpdateAndSetBuffers(CommandList commands, uint index) {
        if (_isDirty) {
            UpdateBuffer(commands, _vertexBuffer, ref _vert, ref _lastVert, !_hasEverUpdated);
            UpdateBuffer(commands, _fragmentBuffer, ref _frag, ref _lastFrag, !_hasEverUpdated);

            _hasEverUpdated = true;
            _isDirty = false;
        }

        commands.SetGraphicsResourceSet(index, _set);

        // This is a pretty hot path (on avg. 1 invocation per shader per frame) so further optimizations might be warranted.
        // 1. We used to calculate the exact portion of the buffer that needs to be updated. Adjusting the SequenceEquals
        // to return the offset where changes begin yielded negative results, likely due to severe pessimization of the SequenceEquals.
        // It might make sense to have another crack at this, since we should get the information *where* we don't change for "free"
        // (at least at the start). To be noted that either way we'd be trading more CPU time for less GPU time.
        // 2. Making it so constant buffers "know" the best way (their most appropriate vector size) to compare themselves should be
        // possible using static interfaces and would allow for cutting down on the branching, likely at the cost of more indirection.
        static unsafe void UpdateBuffer<T>(CommandList commands, DeviceBuffer? buffer, ref T curr, ref T prev, bool force) where T : unmanaged {
            if (buffer == null) {
                return;
            }

            if (force) {
                commands.UpdateBuffer(buffer, 0, ref curr);
                prev = curr;
                return;
            }

            if (!SequenceEqual(ref Unsafe.As<T, byte>(ref curr), ref Unsafe.As<T, byte>(ref prev), (nuint)sizeof(T))) {
                commands.UpdateBuffer(buffer, 0, ref curr);
                prev = curr;
            }
        }
    }

    public bool HasConstant<T>() where T : IConstantMember<T>
        => _vertexBoxed is T || _fragmentBoxed is T;

    public void SetValue<T, TVal>(TVal val) where T : IConstantMember<T, TVal> where TVal : unmanaged, IEquatable<TVal> {
        if (_vertexBoxed is T tVert) {
            if (!tVert.Value.Equals(val)) {
                tVert.Value = val;
                _isDirty = true;
            }

        }
        if (_fragmentBoxed is T tFrag) {
            if (!tFrag.Value.Equals(val)) {
                tFrag.Value = val;
                _isDirty = true;
            }
        }
    }

    protected override void DisposeImpl() {
        _vertexBuffer?.Dispose();
        _fragmentBuffer?.Dispose();
        _set.Dispose();
    }
}
