using System.Diagnostics;
using scpcb.Graphics;
using scpcb.Utility;
using Serilog;
using Serilog.Core;

namespace scpcb;

public class Game : Disposable {
    public GraphicsResources GraphicsResources { get; }

    private IScene _scene;
    public IScene Scene {
        get => _scene;
        set {
            _scene?.Dispose();
            _scene = value;
        }
    }

    public Game(int width, int height) {
        GraphicsResources = new(width, height);

        _scene = new MainScene(GraphicsResources);
    }

    public const int TICK_RATE = 60;
    private const int TICK_GOAL = (int)(TimeSpan.TicksPerSecond / TICK_RATE);

    public void Run() {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        Log.Logger.Information("Hello, world!");

        var countingTo = DateTimeOffset.UtcNow;
        var fps = 0;
        var now = DateTimeOffset.UtcNow;
        var tickAccu = 0;
        while (GraphicsResources.Window.Exists) {
            while (tickAccu < TICK_GOAL) {
                GraphicsResources.Window.PumpEvents();

                var newNow = DateTimeOffset.UtcNow;
                var diff = newNow - now;
                now = newNow;

                _scene.Update(diff.TotalSeconds);

                GraphicsResources.MainTarget.Start();
                var interp = (float)(tickAccu % TICK_GOAL) / TICK_GOAL;
                Debug.Assert(interp is >= 0 and <= 1);
                _scene.Render(GraphicsResources.MainTarget, interp);
                GraphicsResources.MainTarget.End();

                tickAccu += (int)diff.Ticks;

                fps++;
                if (now > countingTo) {
                    countingTo = countingTo.AddSeconds(1);
                    Log.Logger.Information("{Fps} FPS; {TicksToBeDone} TTBD", fps, tickAccu / TICK_GOAL);
                    fps = 0;
                }
            }
            while (tickAccu >= TICK_GOAL) {
                _scene.Tick();
                tickAccu -= TICK_GOAL;
            }
        }
    }

    protected override void DisposeImpl() {
        _scene?.Dispose();
        GraphicsResources.Dispose();
    }
}
