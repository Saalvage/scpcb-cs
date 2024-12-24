using System.Numerics;
using System.Text;

namespace SCPCB.Utility;

public static class BinaryReaderExtensions {
    [ThreadStatic]
    private static WeakReference<StringBuilder>? _sb;

    /// <summary>
    /// ASCII String prefixed by a 32-bit integer length.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    public static string ReadB3DString(this BinaryReader reader) {
        // Implementation stolen from BinaryReader.ReadString()
        // Only difference is only ASCII support so byte count == char count

        var stringLength = reader.ReadInt32();

        switch (stringLength) {
            case < 0:
                throw new IOException("Failed to read B3D string!");
            case 0:
                return "";
        }

        var bufferSize = Math.Min(stringLength, 128);
        Span<byte> charBytes = stackalloc byte[bufferSize];
        Span<char> chars = stackalloc char[bufferSize];

        var currPos = 0;
        StringBuilder? sb = null;
        do {
            var readLength = ((stringLength - currPos) > bufferSize) ? bufferSize : (stringLength - currPos);

            var n = reader.Read(charBytes[..readLength]);
            if (n == 0) {
                throw new IOException("EOF encountered while reading B3D string!");
            }

            Encoding.ASCII.GetChars(charBytes, chars);

            if (currPos == 0) {
                if (n == stringLength) {
                    return new(chars[..n]);
                }

                // Since we could be reading from an untrusted data source, limit the initial size of the
                // StringBuilder instance we're about to get or create. It'll expand automatically as needed.
                if (!_sb?.TryGetTarget(out sb) ?? true) {
                    sb = new(Math.Min(stringLength, 360));
                    _sb = new(sb);
                }
            }

            sb!.Append(chars[..n]);
            currPos += n;
        } while (currPos < stringLength);

        var str = sb.ToString();
        sb.Clear();
        return str;
    }

    public static Vector2 ReadVector2(this BinaryReader reader)
        => new(reader.ReadSingle(), reader.ReadSingle());

    public static Vector3 ReadVector3(this BinaryReader reader)
        => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
}
