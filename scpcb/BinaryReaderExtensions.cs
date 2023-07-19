using System.Numerics;
using SharpDX.Text;

namespace scpcb; 

public static class BinaryReaderExtensions {
    /// <summary>
    /// ASCII String prefixed by a 32-bit integer length.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    public static string ReadB3DString(this BinaryReader reader) {
        var length = reader.ReadInt32();
        // TODO: Optimize if needed, see ReadString impl.
        return Encoding.ASCII.GetString(reader.ReadBytes(length));
    }

    public static Vector2 ReadVector2(this BinaryReader reader)
        => new(reader.ReadSingle(), reader.ReadSingle());

    public static Vector3 ReadVector3(this BinaryReader reader)
        => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
}
