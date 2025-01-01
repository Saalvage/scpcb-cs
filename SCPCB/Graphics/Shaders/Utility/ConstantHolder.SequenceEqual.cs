using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace SCPCB.Graphics.Shaders.Utility;

public partial class ConstantHolder<TVertConstants, TFragConstants> {
    // Taken from SpanHelpers.SequenceEqual, slightly modified.
    // The goto magic (still) has tangible performance benefits, don't remove.
    // Small cases have been moved to the very end because they are highly unusual for constant buffers (we're talking less than a Vector4 here).
    // We also ignore a special case for >=64 (but <=128) bits on 64 bit machines with 128 bit SIMD support.
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
}