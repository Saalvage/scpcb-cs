namespace scpcb.RoomProviders;

public interface IRoomProvider {
    /// <summary>
    /// All lowercase file extensions supported by this provider without leading dot.
    /// </summary>
    public string[] SupportedExtensions { get; }
}
