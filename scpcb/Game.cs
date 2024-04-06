using System.Diagnostics;
using scpcb.Graphics;
using scpcb.Scenes;
using scpcb.Utility;
using Serilog;

namespace scpcb;

public class Game : Disposable {
    public GraphicsResources GraphicsResources { get; }

    private IScene _scene;
    private IScene? _nextScene;
    public IScene Scene {
        get => _scene;
        set {
            Debug.Assert(_nextScene == null);
            _nextScene = value;
        }
    }

    public int Fps { get; private set; }

    public Game(int width, int height) {
        var config = new LoggerConfiguration()
            .MinimumLevel.Debug();
        if (Debugger.IsAttached) {
            config.WriteTo.Console();
        } else {
            config.WriteTo.File("log_latest.txt");
        }
        Log.Logger = config.CreateLogger();

        GraphicsResources = new(width, height);

        _scene = false ? new VideoScene(this, "Assets/Splash_UTG.mp4") : new MainScene(this);
        _scene.OnEnter();
    }

    public const int TICK_RATE = 60;
    private const int TICK_GOAL = (int)(TimeSpan.TicksPerSecond / TICK_RATE);

    public void Run() {
        Log.Information("Hello, world!");

        var countingTo = DateTimeOffset.UtcNow;
        var fps = 0;
        var now = DateTimeOffset.UtcNow;
        var tickAccu = 0;
        while (GraphicsResources.Window.Exists) {
            if (_nextScene != null) {
                _scene.OnLeave();
                _scene.Dispose();
                _scene = _nextScene;
                _scene.OnEnter();
                _nextScene = null;
            }

            while (tickAccu < TICK_GOAL) {
                GraphicsResources.Window.PumpEvents();

                var newNow = DateTimeOffset.UtcNow;
                var diff = newNow - now;
                now = newNow;

                _scene.Update((float)diff.TotalSeconds);

                GraphicsResources.MainTarget.Start();
                var interp = (float)(tickAccu % TICK_GOAL) / TICK_GOAL;
                Debug.Assert(interp is >= 0 and <= 1);

                _scene.Prerender(interp);

                _scene.Render(GraphicsResources.MainTarget, interp);
                GraphicsResources.MainTarget.End();

                tickAccu += (int)diff.Ticks;

                fps++;
                if (now > countingTo) {
                    countingTo = countingTo.AddSeconds(1);
                    Fps = fps;
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
        _scene.OnLeave();
        _scene?.Dispose();
        GraphicsResources.Dispose();
    }
}
