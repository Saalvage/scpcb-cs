using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace SCPCB.Graphics.Shaders.Utility;

public partial class ConstantHolder<TVertConstants, TFragConstants> {
    // Taken from SpanHelpers.SequenceEqual, slightly modified.
    // Small cases have been moved to the very end because they are highly unusual for constant buffers (we're talking less than a Vector4 here).
    // We also ignore a special case for >=64 (but <=128) bits on 64 bit machines with 128 bit SIMD support.
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static unsafe bool SequenceEqual(ref byte first, ref byte second, nuint length) {
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
                            return false;
                        }
                        offset += (nuint)Vector512<byte>.Count;
                    } while (lengthToExamine > offset);
                }

                // Do final compare as Vector512<byte>.Count from end rather than start
                if (Vector512.LoadUnsafe(ref first, lengthToExamine) ==
                    Vector512.LoadUnsafe(ref second, lengthToExamine)) {
                    // C# compiler inverts this test, making the outer goto the conditional jmp.
                    return true;
                }

                // This becomes a conditional jmp forward to not favor it.
                return false;
            } else if (Vector256.IsHardwareAccelerated && length >= (nuint)Vector256<byte>.Count) {
                nuint offset = 0;
                nuint lengthToExamine = length - (nuint)Vector256<byte>.Count;
                // Unsigned, so it shouldn't have overflowed larger than length (rather than negative)
                Debug.Assert(lengthToExamine < length);
                if (lengthToExamine != 0) {
                    do {
                        if (Vector256.LoadUnsafe(ref first, offset) !=
                            Vector256.LoadUnsafe(ref second, offset)) {
                            return false;
                        }
                        offset += (nuint)Vector256<byte>.Count;
                    } while (lengthToExamine > offset);
                }

                // Do final compare as Vector256<byte>.Count from end rather than start
                if (Vector256.LoadUnsafe(ref first, lengthToExamine) ==
                    Vector256.LoadUnsafe(ref second, lengthToExamine)) {
                    // C# compiler inverts this test, making the outer goto the conditional jmp.
                    return true;
                }

                // This becomes a conditional jmp forward to not favor it.
                return false;
            } else if (length >= (nuint)Vector128<byte>.Count) {
                nuint offset = 0;
                nuint lengthToExamine = length - (nuint)Vector128<byte>.Count;
                // Unsigned, so it shouldn't have overflowed larger than length (rather than negative)
                Debug.Assert(lengthToExamine < length);
                if (lengthToExamine != 0) {
                    do {
                        if (Vector128.LoadUnsafe(ref first, offset) !=
                            Vector128.LoadUnsafe(ref second, offset)) {
                            return false;
                        }
                        offset += (nuint)Vector128<byte>.Count;
                    } while (lengthToExamine > offset);
                }

                // Do final compare as Vector128<byte>.Count from end rather than start
                if (Vector128.LoadUnsafe(ref first, lengthToExamine) ==
                    Vector128.LoadUnsafe(ref second, lengthToExamine)) {
                    // C# compiler inverts this test, making the outer goto the conditional jmp.
                    return true;
                }

                // This becomes a conditional jmp forward to not favor it.
                return false;
            }
        }

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
                        return false;
                    }
                    offset += (nuint)sizeof(nuint);
                } while (lengthToExamine > offset);
            }

            // Do final compare as sizeof(nuint) from end rather than start
            return (Unsafe.ReadUnaligned<nuint>(ref Unsafe.AddByteOffset(ref first, lengthToExamine))
                    == Unsafe.ReadUnaligned<nuint>(ref Unsafe.AddByteOffset(ref second, lengthToExamine)));
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
            return differentBits == 0;
        } else {
            nuint offset = length - sizeof(uint);
            uint differentBits = Unsafe.ReadUnaligned<uint>(ref first) - Unsafe.ReadUnaligned<uint>(ref second);
            differentBits |= Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref first, offset)) - Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref second, offset));
            return differentBits == 0;
        }
    }
}