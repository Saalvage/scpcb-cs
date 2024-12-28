using System.Drawing;
using System.Numerics;
using BepuPhysics.Collidables;
using SCPCB.Entities.Items;
using SCPCB.Graphics;
using SCPCB.Graphics.Caches;
using SCPCB.Graphics.ModelCollections;
using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.Shaders.Vertices;
using SCPCB.Graphics.Textures;
using SCPCB.Graphics.UserInterface;
using SCPCB.Graphics.UserInterface.Composites;
using SCPCB.Graphics.UserInterface.Menus;
using SCPCB.Graphics.UserInterface.Primitives;
using SCPCB.Graphics.UserInterface.Utility;
using SCPCB.Map;
using SCPCB.Physics;
using SCPCB.Physics.Primitives;
using SCPCB.PlayerController;
using SCPCB.Serialization;
using SCPCB.Utility;
using Veldrid;
using Veldrid.Sdl2;

namespace SCPCB.Scenes;

public class MainScene : Scene3D {
    private readonly Game _game;
    private readonly GraphicsResources _gfxRes;
    private readonly InputManager _input;
    
    private readonly Player _player;

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

    private readonly ModelImageGeneratorMenu _mig;

    private readonly Font _font;

    private readonly TextElement _str;

    private bool _uiDebug = false;
    private float _uiDebugHue = 0f;

    private bool _paused = false;
    private IUIElement? _openMenu = null;

    private readonly Dictionary<string, IRoomData> _rooms;

    public MainScene(Game game, PlacedRoomInfo?[,]? map = null) : base(game.GraphicsResources) {
        _game = game;
        _gfxRes = game.GraphicsResources;
        _input = game.InputManager;

        AddEntity(Physics);

        _player = new(this);
        Camera = _player.Camera;
        AddEntity(_player);

        _renderTexture = new(_gfxRes, 100, 100, true);

        _font = _gfxRes.FontCache.GetFont("Assets/Fonts/Courier New.ttf", 32);

        foreach (var c in Enumerable.Range(0, 1000)) {
            _font.GetGlyphInfo((char)c);
        }

        var gfx = _gfxRes.GraphicsDevice;
        var window = _gfxRes.Window;

        var video = new Video(_gfxRes, "Assets/Splash_UTG.mp4");
        video.Loop = true;
        AddEntity(video);

        var ui = new UIManager(_gfxRes, _input);
        AddEntity(ui);

        var uiElem = new TextureElement(_gfxRes, _gfxRes.TextureCache.GetTexture(Color.MidnightBlue));
        uiElem.Alignment = Alignment.TopRight;
        uiElem.Position = new(-10, 0);
        uiElem.PixelSize = new(500, 50);
        ui.Root.AddChild(uiElem);

        var uiElem2 = new TextureElement(_gfxRes, _renderTexture);
        uiElem2.Alignment = Alignment.BottomRight;
        //uiElem2.PixelSize *= 0.1f;
        ui.Root.AddChild(uiElem2);

        _str = new(_gfxRes, _font);
        _str.Text = "T\nBla bla y_\n^";
        _str.Alignment = Alignment.TopLeft;
        _str.Scale *= 0.8f;
        ui.Root.Children[0].AddChild(_str);

        ui.Root.AddChild(new Button(_gfxRes, ui, "Hello", 0, 0, 0) {
            Alignment = Alignment.BottomRight,
        });

        ui.Root.AddChild(new InputBox(_gfxRes, ui, _input, _font));

        _hud = new(_player, ui);
        AddEntity(_hud);

        // TODO: Remove, necessary for now because item creation looks for the physics object in the scene.
        DealWithEntityBuffers();

        var reg = new ItemRegistry(_gfxRes, this);
        reg.RegisterItemsFromFile("Assets/Items/items.txt");
        var itemmm = reg.CreateItem(new(_player.Camera.Position, Quaternion.Identity), "doc173");
        AddEntity(itemmm);
        _player.PickItem(itemmm);

        _gfxRes.ShaderCache.SetGlobal<IProjectionMatrixConstantMember, Matrix4x4>(
            Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 180 * 90, (float)window.Width / window.Height, 0.1f, 100f));

        _gfxRes.ShaderCache.SetGlobal<IUIProjectionMatrixConstantMember, Matrix4x4>(
            Matrix4x4.CreateOrthographic(_gfxRes.Window.Width, _gfxRes.Window.Height, -100, 100));
        
        Sdl2Native.SDL_SetRelativeMouseMode(true);

        var modelShader = _gfxRes.ShaderCache.GetShader<ModelShader, VPositionTexture>();

        _mig = new(_gfxRes, ui, _input, Physics);
        _mig.IsVisible = false;
        ui.Root.AddChild(_mig);

        var coolTexture = _gfxRes.TextureCache.GetTexture("Assets/173texture.jpg");
        _logoMat = _gfxRes.MaterialCache.GetMaterial(modelShader, [video.Texture], [gfx.PointSampler]);

        _otherMat = _gfxRes.MaterialCache.GetMaterial(modelShader, [coolTexture], [gfx.PointSampler]);

        _renderMat = _gfxRes.MaterialCache.GetMaterial(modelShader, [_renderTexture], [gfx.PointSampler]);

        var billboard = Billboard.Create(_gfxRes, _renderTexture);
        billboard.Transform = billboard.Transform with {
            Position = new(2, 2, -0.1f),
        };
        AddEntity(billboard);

        _rooms = map.Cast<PlacedRoomInfo?>()
            .Where(x => x != null)
            .Select(x => x.Room.Mesh)
            .Distinct()
            .ToDictionary(x => x, x => _gfxRes.LoadRoom(this, Physics, "Assets/Rooms/" + x));
        for (var x = 0; x < map.GetLength(0); x++) {
            for (var y = 0; y < map.GetLength(1); y++) {
                var info = map[x, y];
                if (info != null) {
                    var room = _rooms[info.Room.Mesh].Instantiate(new(x * 20.5f, 0, y * 20.5f),
                        Quaternion.CreateFromYawPitchRoll(info.Direction.ToRadians() + MathF.PI, 0, 0));
                    AddEntity(room);
                }
            }
        }

        _cacheEntry = Physics.ModelCache.GetModel("Assets/173_2.b3d");
        _scp173 = _cacheEntry.Models.Instantiate().OfType<ICBModel<VPositionTexture>>().First();
        _hull = _cacheEntry.Collision;

        window.KeyDown += HandleKeyDown;
        window.MouseDown += HandleMouseEvent;
        window.MouseUp += HandleMouseEvent;
    }

    public override void Update(float delta) {
        if (!_paused) {
            if (_gfxRes.Window.MouseDelta != Vector2.Zero) {
                _player.HandleMouse(_gfxRes.Window.MouseDelta * 0.01f);
            }

            var dir = Vector2.Zero;
            if (_input.IsKeyDown(Key.W)) dir += Vector2.UnitY;
            if (_input.IsKeyDown(Key.S)) dir -= Vector2.UnitY;
            if (_input.IsKeyDown(Key.A)) dir += Vector2.UnitX;
            if (_input.IsKeyDown(Key.D)) dir -= Vector2.UnitX;

            _player.MoveDir = dir == Vector2.Zero ? Vector2.Zero : Vector2.Normalize(dir);
            _player.IsSprinting = _input.IsKeyDown(Key.ShiftLeft);
        }

        if (_uiDebug) {
            _uiDebugHue = (_uiDebugHue + delta * 50) % 360;
            AttachDebugBorders();
        }

        _str.Text = _game.Fps.ToString();

        base.Update(delta);
    }

    public override void Render(IRenderTarget target, float interp) {
        // TODO: This should offer great opportunities for optimization & parallelization!
        // On second consideration: Most render targets will differ in e.g. view position
        // meaning potential for optimization might not actually be there. :(

        // TODO: This also messes with objects that utilize non-instance shader constants
        // on a per-instance basis (e.g. for objects intended to be very lightweight (UI components))
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
        _gfxRes.Window.MouseDown -= HandleMouseEvent;
        _gfxRes.Window.MouseUp -= HandleMouseEvent;
    }

    private static string? _serialized;

    private void HandleKeyDown(KeyEvent e) {
        switch (e.Key) {
            case Key.Space: {
                var body = _hull.CreateDynamic(new(_player.Camera.Position, _player.Camera.Rotation), 1);
                body.Velocity = new(10 * Vector3.Transform(new(0, 0, 1), _player.Camera.Rotation));
                AddEntity(new PhysicsModelCollection(Physics, body, [
                    new CBModel<VPositionTexture>(
                    _gfxRes.ShaderCache.GetShader<ModelShader, VPositionTexture>().TryCreateInstanceConstants(), Random.Shared.Next(3) switch {
                        0 => _renderMat,
                        1 => _otherMat,
                        2 => _logoMat,
                    }, _scp173.Mesh),
                ]));
                break;
            }
            case Key.Escape:
                if (_openMenu != null) {
                    SetOpenMenu(null);
                } else {
                    _game.Scene = new VideoScene(_game, "Assets/Splash_UTG.mp4");
                }
                break;
            case Key.AltLeft: {
                var from = _player.Camera.Position;
                var to = from + Vector3.Transform(Vector3.UnitZ, _player.Camera.Rotation) * 5f;
                var line = new DebugLine(_gfxRes, from, to);
                line.Color = Physics.RayCastVisible(from, to) is not null ? new(1, 0, 0) : new(0, 1, 0);
                AddEntity(line);
                break;
            }
            case Key.F2: {
                if (_openMenu == _mig) {
                    SetOpenMenu(null);
                } else {
                    SetOpenMenu(_mig);
                }
                break;
            }
            case Key.F5: {
                _serialized = SerializationHelper.SerializeTest(GetEntitiesOfType<ISerializableEntity>());
                foreach (var i in GetEntitiesOfType<ISerializableEntity>()) {
                    //RemoveEntity(i);
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
            case Key.Tab:
                if (_openMenu == _hud.Inventory) {
                    SetOpenMenu(null);
                } else {
                    SetOpenMenu(_hud.Inventory);
                }
                break;
        }
    }

    // TODO: Turn into property?
    public void SetOpenMenu(IUIElement? newOpenMenu) {
        if (_openMenu != null) {
            _openMenu.IsVisible = false;
        }
        _openMenu = newOpenMenu;
        if (_openMenu != null) {
            _openMenu.IsVisible = true;
        }

        _paused = _openMenu != null;

        _input.SetMouseCaptured(!_paused);
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

    private void HandleMouseEvent(MouseEvent e) {
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
        foreach (var r in _rooms.Values) {
            r.Dispose();
        }
        _renderMat.Dispose();
        _renderTexture.Dispose();
        base.DisposeImpl();
    }
}
