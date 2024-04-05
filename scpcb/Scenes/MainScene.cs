using System.Drawing;
using System.Numerics;
using BepuPhysics.Collidables;
using scpcb.Entities.Items;
using scpcb.Graphics;
using scpcb.Graphics.Caches;
using scpcb.Graphics.ModelCollections;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Graphics.Shaders.Vertices;
using scpcb.Graphics.Textures;
using scpcb.Graphics.UserInterface;
using scpcb.Map;
using scpcb.Physics;
using scpcb.Physics.Primitives;
using scpcb.PlayerController;
using scpcb.Serialization;
using scpcb.Utility;
using Veldrid;
using Veldrid.Sdl2;

namespace scpcb.Scenes;

public class MainScene : Scene3D {
    private readonly Game _game;
    private readonly GraphicsResources _gfxRes;
    
    private readonly Player _player;

    private readonly Dictionary<Key, bool> _keysDown = [];
    private bool KeyDown(Key x) => _keysDown.TryGetValue(x, out var y) && y;

    private readonly Dictionary<MouseButton, bool> _mouseButtonsDown = [];
    private bool MouseButtonDown(MouseButton x) => _mouseButtonsDown.TryGetValue(x, out var y) && y;

    private readonly ICBShape<ConvexHull> _hull;
    private readonly ICBMaterial<VPositionTexture> _renderMat;
    private readonly ICBMaterial<VPositionTexture> _otherMat;
    private readonly ICBMaterial<VPositionTexture> _logoMat;
    private readonly ICBModel<VPositionTexture> _scp173;

    private readonly IRoomData _room008;
    private readonly IRoomData _room895;
    private readonly IRoomData _room4Tunnels;

    private readonly RenderTexture _renderTexture;
    private readonly ModelCache.CacheEntry _cacheEntry;

    private readonly HUD _hud;

    private readonly Font _font;

    private readonly TextElement _str;

    private bool _uiDebug = false;
    private float _uiDebugHue = 0f;

    public MainScene(Game game) : base(game.GraphicsResources) {
        _game = game;
        _gfxRes = game.GraphicsResources;

        AddEntity(Physics);

        _player = new(this);
        Camera = _player.Camera;
        AddEntity(_player);

        _renderTexture = new(_gfxRes, 100, 100, true);

        _font = _gfxRes.LoadFont("Assets/Fonts/Courier New.ttf", 64);

        foreach (var c in Enumerable.Range(0, 1000)) {
            _font.GetGlyphInfo((char)c);
        }

        var gfx = _gfxRes.GraphicsDevice;
        var window = _gfxRes.Window;

        var video = new Video(_gfxRes, "Assets/Splash_UTG.mp4");
        video.Loop = true;
        AddEntity(video);

        var ui = new UIManager(_gfxRes);
        AddEntity(ui);

        var uiElem = new TextureElement(_gfxRes, _gfxRes.TextureCache.GetTexture(Color.Aqua));
        uiElem.Alignment = Alignment.TopRight;
        uiElem.Position = new(-10, 0);
        uiElem.PixelSize = new(500, 50);
        ui.Root.AddChild(uiElem);

        var uiElem2 = new TextureElement(_gfxRes, _renderTexture);
        uiElem2.Alignment = Alignment.BottomRight;
        //uiElem2.PixelSize *= 0.1f;
        ui.Root.AddChild(uiElem2);

        _str = new(_gfxRes, _font);
        _str.Text = "^Hxy^";
        _str.Alignment = Alignment.TopLeft;
        _str.Scale *= 0.8f;
        ui.Root.Children[0].AddChild(_str);

        _hud = new(_player, ui);
        AddEntity(_hud);

        var manager = new ItemManager(_gfxRes, Physics, this);
        var test = manager.CreateItem("GasMask", new(_player.Camera.Position, Quaternion.Identity));
        AddEntity(test);
        _player.PickItem(test);

        _gfxRes.ShaderCache.SetGlobal<IProjectionMatrixConstantMember, Matrix4x4>(
            Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 180 * 90, (float)window.Width / window.Height, 0.1f, 100f));

        _gfxRes.ShaderCache.SetGlobal<IUIProjectionMatrixConstantMember, Matrix4x4>(
            Matrix4x4.CreateOrthographic(_gfxRes.Window.Width, _gfxRes.Window.Height, -100, 100));
        
        Sdl2Native.SDL_SetRelativeMouseMode(true);

        var modelShader = _gfxRes.ShaderCache.GetShader<ModelShader, VPositionTexture>();

        var coolTexture = _gfxRes.TextureCache.GetTexture("Assets/173texture.jpg");
        _logoMat = _gfxRes.MaterialCache.GetMaterial(modelShader, video.Texture.AsEnumerableElement(),
            gfx.PointSampler.AsEnumerableElement());

        _otherMat = _gfxRes.MaterialCache.GetMaterial(modelShader, coolTexture.AsEnumerableElement(),
            gfx.PointSampler.AsEnumerableElement());

        _renderMat = _gfxRes.MaterialCache.GetMaterial(modelShader, _renderTexture.AsEnumerableElement(),
            gfx.PointSampler.AsEnumerableElement());

        var billboard = Billboard.Create(_gfxRes, _renderTexture);
        billboard.Transform = billboard.Transform with {
            Position = new(-2, 2, -0.1f),
            Rotation = Quaternion.CreateFromYawPitchRoll(MathF.PI, 0, 0),
        };
        AddEntity(billboard);

        // TODO: Remove call and make method private again.
        DealWithEntityBuffers();

        _room008 = _gfxRes.LoadRoom(this, Physics, "Assets/Rooms/008/008_opt.rmesh");
        _room895 = _gfxRes.LoadRoom(this, Physics, "Assets/Rooms/coffin/coffin_opt.rmesh");
        _room4Tunnels = _gfxRes.LoadRoom(this, Physics, "Assets/Rooms/4tunnels/4tunnels_opt.rmesh");
        foreach (var i in Enumerable.Range(0, 5)) {
            foreach (var j in Enumerable.Range(0, 10)) {
                var room = (i == 0 || i == 4 || j == 0 || j == 9 ? _room008 : i == 2 || j == 5 ? _room895 : _room4Tunnels)
                    .Instantiate(new(j * -20.5f, 0, i * -20.5f),
                        Quaternion.CreateFromYawPitchRoll(((i + j) % 4) * MathF.PI / 2f, 0, 0));
                AddEntity(room);
            }
        }

        _cacheEntry = Physics.ModelCache.GetModel("Assets/173_2.b3d");
        _scp173 = _cacheEntry.Models.Instantiate().OfType<ICBModel<VPositionTexture>>().First();
        _hull = _cacheEntry.Collision;

        window.KeyDown += HandleKeyDown;
        window.KeyUp += HandleKeyUp;
        window.MouseDown += HandleMouseEvent;
        window.MouseUp += HandleMouseEvent;
    }

    public override void Update(float delta) {
        if (_gfxRes.Window.MouseDelta != Vector2.Zero) {
            _player.HandleMouse(_gfxRes.Window.MouseDelta * 0.01f);
        }

        var dir = Vector2.Zero;
        if (KeyDown(Key.W)) dir += Vector2.UnitY;
        if (KeyDown(Key.S)) dir -= Vector2.UnitY;
        if (KeyDown(Key.A)) dir += Vector2.UnitX;
        if (KeyDown(Key.D)) dir -= Vector2.UnitX;

        _player.MoveDir = dir == Vector2.Zero ? Vector2.Zero : Vector2.Normalize(dir);
        _player.IsSprinting = KeyDown(Key.ShiftLeft);

        if (_uiDebug) {
            _uiDebugHue = (_uiDebugHue + delta * 50) % 360;
            AttachDebugBorders();
        }

        base.Update(delta);
    }

    public override void Render(IRenderTarget target, float interp) {
        // TODO: This should offer great opportunities for optimization & parallelization!
        // On second consideration: Most render targets will differ in e.g. view position
        // meaning potential for optimization might not actually be there. :(

        // TODO: This also messes with objects that utilize non-instance shader constants
        // on a per-instance basis (e.g. for objects intended to be very light weight (UI components))
        // which is why it's been disabled for now. Either the usage of shader constants in the manner
        // described above or the parallelism here needs to be revisited.

        _renderTexture.Start();
        //var a = Task.Run(() => {
            base.Render(_renderTexture, interp);
            _renderTexture.End();
        //});
        //var b = Task.Run(() => {
            base.Render(target, interp);
        //});
        //Task.WaitAll(a, b);
    }

    public override void OnLeave() {
        // TODO: This sucks! Might as well eliminate the entire method.
        _gfxRes.Window.KeyDown -= HandleKeyDown;
        _gfxRes.Window.KeyUp -= HandleKeyUp;
        _gfxRes.Window.MouseDown -= HandleMouseEvent;
        _gfxRes.Window.MouseUp -= HandleMouseEvent;
    }

    private static string? _serialized;

    private void HandleKeyDown(KeyEvent e) {
        _keysDown[e.Key] = true;

        switch (e.Key) {
            case Key.Space: {
                var body = _hull.CreateDynamic(new(_player.Camera.Position, _player.Camera.Rotation), 1);
                body.Velocity = new(10 * Vector3.Transform(new(0, 0, 1), _player.Camera.Rotation));
                AddEntity(new PhysicsModelCollection(Physics, body, new[] { new CBModel<VPositionTexture>(
                    _gfxRes.ShaderCache.GetShader<ModelShader, VPositionTexture>().TryCreateInstanceConstants(), Random.Shared.Next(3) switch {
                        0 => _renderMat,
                        1 => _otherMat,
                        2 => _logoMat,
                    }, _scp173.Mesh)}));
                break;
            }
            case Key.Escape:
                _game.Scene = new VideoScene(_game, "Assets/Splash_UTG.mp4");
                break;
            case Key.AltLeft: {
                var from = _player.Camera.Position;
                var to = from + Vector3.Transform(Vector3.UnitZ, _player.Camera.Rotation) * 5f;
                var line = new DebugLine(_gfxRes, from, to);
                line.Color = Physics.RayCastVisible(from, to) ? new(1, 0, 0) : new(0, 1, 0);
                AddEntity(line);
                break;
            }
            case Key.F5: {
                _serialized = SerializationHelper.SerializeTest(GetEntitiesOfType<ISerializableEntity>());
                foreach (var i in GetEntitiesOfType<ISerializableEntity>()) {
                    RemoveEntity(i);
                }
                break;
            }
            case Key.BackSpace: {
                if (_serialized != null) {
                    AddEntities(SerializationHelper.DeserializeTest(_serialized, _gfxRes, this));
                }
                break;
            }
            case Key.Comma:
                _str.Text += (char)Random.Shared.Next(256);
                break;
            case Key.P:
                _uiDebug = !_uiDebug;
                AttachDebugBorders();
                break;
        }
    }

    private void AttachDebugBorders() {
        AttachDebugBordersRecursive(GetEntitiesOfType<UIManager>().Single().Root, _uiDebug);

        void AttachDebugBordersRecursive(IUIElement elem, bool add) {
            var count = elem.Children.Count;
            for (var i = 0; i < count; i++) {
                if (elem.Children[i] is DebugBorder) {
                    elem.RemoveChild(elem.Children[i]);
                    count--;
                    i--;
                    continue;
                }
                AttachDebugBordersRecursive(elem.Children[i], add);
                if (add) {
                    elem.Children[i].AddChild(new DebugBorder(_gfxRes, elem.Children[i].PixelSize, 1f, Color.White) {
                        Color = Helpers.ColorFromHSV(_uiDebugHue, 1f, 1f),
                    });
                }
            }
        }
    }

    private void HandleKeyUp(KeyEvent e) {
        _keysDown[e.Key] = false;
    }

    private void HandleMouseEvent(MouseEvent e) {
        _mouseButtonsDown[e.MouseButton] = e.Down;

        if (e.Down) {
            switch (e.MouseButton) {
                case MouseButton.Left:
                    _player.TryPick();
                    break;
                case MouseButton.Right:
                    _hud.ClearItem();
                    break;
            }
        }
    }

    protected override void DisposeImpl() {
        _room008.Dispose();
        _room4Tunnels.Dispose();
        _renderMat.Dispose();
        _renderTexture.Dispose();
        base.DisposeImpl();
    }
}
