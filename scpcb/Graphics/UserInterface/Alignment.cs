namespace scpcb.Graphics.UserInterface;

public record struct Alignment(Alignment.Vertical Verticality, Alignment.Horizontal Horizontality) {
    public enum Vertical : byte { Top, Center, Bottom }
    public enum Horizontal : byte { Left, Center, Right }

    public static Alignment TopLeft = new(Vertical.Top, Horizontal.Left);
    public static Alignment TopCenter = new(Vertical.Top, Horizontal.Center);
    public static Alignment TopRight = new(Vertical.Top, Horizontal.Right);

    public static Alignment CenterLeft = new(Vertical.Center, Horizontal.Left);
    public static Alignment Center = new(Vertical.Center, Horizontal.Center);
    public static Alignment CenterRight = new(Vertical.Center, Horizontal.Right);

    public static Alignment BottomLeft = new(Vertical.Bottom, Horizontal.Left);
    public static Alignment BottomCenter = new(Vertical.Bottom, Horizontal.Center);
    public static Alignment BottomRight = new(Vertical.Bottom, Horizontal.Right);
}
