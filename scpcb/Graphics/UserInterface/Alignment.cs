namespace scpcb.Graphics.UserInterface;

public record struct Alignment(Alignment.Vertical Verticality, Alignment.Horizontal Horizontality) {
    public enum Vertical : byte { Top, Center, Bottom }
    public enum Horizontal : byte { Left, Center, Right }

    public static readonly Alignment TopLeft = new(Vertical.Top, Horizontal.Left);
    public static readonly Alignment TopCenter = new(Vertical.Top, Horizontal.Center);
    public static readonly Alignment TopRight = new(Vertical.Top, Horizontal.Right);

    public static readonly Alignment CenterLeft = new(Vertical.Center, Horizontal.Left);
    public static readonly Alignment Center = new(Vertical.Center, Horizontal.Center);
    public static readonly Alignment CenterRight = new(Vertical.Center, Horizontal.Right);

    public static readonly Alignment BottomLeft = new(Vertical.Bottom, Horizontal.Left);
    public static readonly Alignment BottomCenter = new(Vertical.Bottom, Horizontal.Center);
    public static readonly Alignment BottomRight = new(Vertical.Bottom, Horizontal.Right);
}
