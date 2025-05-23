﻿using System.Diagnostics;
using SCPCB.Audio;
using SCPCB.B;
using SCPCB.Graphics;
using SCPCB.Scenes;
using SCPCB.Utility;

namespace SCPCB;

public class Game : Disposable {
    public GraphicsResources GraphicsResources { get; }
    public AudioResources AudioResources { get; } = new();
    public InputManager InputManager { get; }

    private IScene _scene;
    private IScene? _nextScene;
    public IScene Scene {
        get => _scene;
        set {
            Debug.Assert(_nextScene == null);
            _nextScene = value;
        }
    }

    public int FPS { get; private set; }

    public Game(int width, int height) {
        var config = new LoggerConfiguration()
            .MinimumLevel.Information();
        if (Debugger.IsAttached) {
            config.WriteTo.Console();
        } else {
            config.WriteTo.File("log_latest.txt");
        }
        Log.Logger = config.CreateLogger();

        GraphicsResources = new(width, height);
        InputManager = new(GraphicsResources.Window);

        _scene = new BScene(this);
        //_scene = new CBScene(this, Helpers.GenerateDebugRooms());
        _scene.OnEnter();
    }

    public const int TICK_RATE = 60;
    private const int TICK_GOAL = (int)(TimeSpan.TicksPerSecond / TICK_RATE);
    public const float TICK_DELTA = 1f / TICK_RATE;

    public void Run() {
        Log.Information("Hello, world!");

        var countingTo = DateTimeOffset.UtcNow;
        var fps = 0;
        var now = DateTimeOffset.UtcNow;
        var tickAccu = 0;
        while (GraphicsResources.Window.Exists) {
            while (tickAccu < TICK_GOAL) {
                InputManager.PumpEvents();

                // TODO: This used to be in the outer loop before pumping the events, that caused issues with
                // constants not being applied correctly in scene ctors.
                // This is just a bandaid fix, but we all know it will probably stick around until the end of time.
                if (_nextScene != null) {
                    _scene.OnLeave();
                    _scene.Dispose();
                    _scene = _nextScene;
                    _scene.OnEnter();
                    _nextScene = null;
                }

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
                    FPS = fps;
                    fps = 0;
                }
            }
            while (tickAccu >= TICK_GOAL) {
                _scene.Tick();
                AudioResources.Tick();
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
