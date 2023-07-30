using System.Diagnostics;
using scpcb.Graphics;
using Veldrid;

namespace scpcb; 

public class Game : Disposable {
    public GraphicsResources Graphics { get; }

    private readonly CommandList _commandsList;

    private IScene _scene;
    public IScene Scene {
        get => _scene;
        set {
            _scene?.Dispose();
            _scene = value;
        }
    }

    public Game(int width, int height) {
        Graphics = new(width, height);

        _scene = new MainScene(Graphics);

        _commandsList = Graphics.GraphicsDevice.ResourceFactory.CreateCommandList();
    }

    public const int TICK_RATE = 60;
    private const int TICK_GOAL = (int)(TimeSpan.TicksPerSecond / TICK_RATE);

    public void Run() {
        var gfx = Graphics.GraphicsDevice;
        var countingTo = DateTimeOffset.UtcNow;
        var fps = 0;
        var now = DateTimeOffset.UtcNow;
        var tickAccu = 0;
        while (Graphics.Window.Exists) {
            while (tickAccu < TICK_GOAL) {
                Graphics.Window.PumpEvents();

                var newNow = DateTimeOffset.UtcNow;
                var diff = newNow - now;
                now = newNow;

                _scene.Update(diff.TotalSeconds);

                _commandsList.Begin();
                _commandsList.SetFramebuffer(gfx.SwapchainFramebuffer);
                _commandsList.ClearColorTarget(0, RgbaFloat.Grey);
                _commandsList.ClearDepthStencil(1);
                var interp = (float)(tickAccu % TICK_GOAL) / TICK_GOAL;
                Debug.Assert(interp is >= 0 and <= 1);
                _scene.Render(_commandsList, interp);
                _commandsList.End();
                gfx.SubmitCommands(_commandsList);
                gfx.SwapBuffers();

                tickAccu += (int)diff.Ticks;

                fps++;
                if (now > countingTo) {
                    countingTo = countingTo.AddSeconds(1);
                    Console.WriteLine($"{fps} {tickAccu / TICK_GOAL}");
                    fps = 0;
                }
            }
            _scene.Tick();
            tickAccu -= TICK_GOAL;
        }
    }

    protected override void DisposeImpl() {
        _scene?.Dispose();
        _commandsList.Dispose();
        Graphics.Dispose();
    }
}
