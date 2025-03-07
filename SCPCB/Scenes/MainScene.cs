using System.Drawing;
using System.Numerics;
using BepuPhysics.Collidables;
using SCPCB.Audio;
using SCPCB.B;
using SCPCB.Entities.Items;
using SCPCB.Graphics;
using SCPCB.Graphics.Animation;
using SCPCB.Graphics.DebugUtilities;
using SCPCB.Graphics.Models;
using SCPCB.Graphics.ModelTemplates;
using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.Shaders.Vertices;
using SCPCB.Graphics.Text;
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
using ShaderGen;
using Veldrid;
using Veldrid.Sdl2;

namespace SCPCB.Scenes;

public class MainScene : Scene3D {
    private readonly Game _game;
    private readonly InputManager _input;

    protected readonly Player _player;

    private readonly ICBMaterial<VPositionTexture> _renderMat;
    private readonly ICBMaterial<VPositionTexture> _otherMat;
    private readonly ICBMaterial<VPositionTexture> _logoMat;

    private readonly DreamFilter _dreamFilter;

    private readonly PhysicsModelTemplate _template;

    private readonly HUD _hud;

    private readonly ModelImageGeneratorMenu _mig;

    private readonly Font _font;

    private readonly TextElement _str;

    private bool _uiDebug = false;
    private float _uiDebugHue = 0f;

    private bool _paused = false;
    private IUIElement? _openMenu = null;

    private Vector3? _measuringTape;

    public MainScene(Game game, Player.CollisionInfo playerCollisionInfo) : base(game.GraphicsResources, game.AudioResources) {
        _game = game;
        _input = game.InputManager;

        AddEntity(Physics);

        DealWithEntityBuffers();

        _player = new(this, playerCollisionInfo);
        Camera = _player.Camera;
        _player.Noclip = true;
        _player.Camera.WorldTransform = new(new(-2.5f, -0.699f, 0.5f), Quaternion.CreateFromYawPitchRoll(MathF.PI, 0, 0));

        AddEntity(_player);

        DealWithEntityBuffers();

        _font = Graphics.FontCache.GetFont("Assets/Fonts/Courier New.ttf", 32);

        foreach (var c in Enumerable.Range(0, 1000)) {
            _font.GetGlyphInfo((char)c);
        }

        AddEntity(new TextModel3D(Graphics, _font, "test COOL!\nover\nmultiple lines!") {
            WorldTransform = new(Vector3.Zero, Quaternion.Identity, new(0.01f)),
        });

        var gfx = Graphics.GraphicsDevice;
        var window = Graphics.Window;

        var video = new Video(Graphics, "Assets/Splash_UTG.mp4");
        video.Loop = true;
        AddEntity(video);

        var ui = new UIManager(Graphics, _input);
        AddEntity(ui);

        ui.Root.AddChild(new Button(Graphics, ui, "Hello", 0, 0, 0) {
            Alignment = Alignment.BottomRight,
        });

        IUIElement curr = ui.Root;
        foreach (var i in Enumerable.Range(0, 10)) {
            var child = new TextureElement(Graphics, Graphics.TextureCache.GetTexture(Helpers.ColorFromHSV(i * 36, 1, 1))) {
                PixelSize = new((10 - i) * 10),
                Alignment = (i % 4) switch {
                    0 => Alignment.TopRight,
                    1 => Alignment.BottomRight,
                    2 => Alignment.BottomLeft,
                    3 => Alignment.TopLeft,
                },
            };
            curr.AddChild(child);
            curr = child;
        }

        ui.Root.AddChild(new InputBox(Graphics, ui, _input, _font) { ConstrainContentsToSize = true });

        _str = new(Graphics, _font) {
            Text = "T\nBla bla y_\n^",
            Position = new(5),
            Scale = new(0.8f),
            Z = 1,
        };
        ui.Root.AddChild(_str);

        _hud = new(_player, ui);
        AddEntity(_hud);

        var reg = new ItemRegistry(this);
        reg.RegisterItemsFromFile("Assets/Items/items.txt");
        var itemmm = reg.CreateItem(new(_player.Camera.WorldTransform.Position, Quaternion.Identity, new(0.1f)), "doc173");
        AddEntity(itemmm);
        _player.PickItem(itemmm);

        Graphics.ShaderCache.SetGlobal<IProjectionMatrixConstantMember, Matrix4x4>(
            Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 180 * 58.69f, (float)window.Width / window.Height, 0.1f, 100f));

        Graphics.ShaderCache.SetGlobal<IUIProjectionMatrixConstantMember, Matrix4x4>(
            Helpers.CreateUIProjectionMatrix(Graphics.Window.Width, Graphics.Window.Height));

        Graphics.ShaderCache.SetGlobal<IAmbientLightConstantMember, float>(40f / 255);

        Sdl2Native.SDL_SetRelativeMouseMode(true);

        var modelShader = Graphics.ShaderCache.GetShader<ModelShader, VPositionTexture>();

        _mig = new(Graphics, ui, _input, Physics);
        _mig.IsVisible = false;
        ui.Root.AddChild(_mig);

        _dreamFilter = new(Graphics, base.Render) {
            TicksPerCycle = 1,
            BlurFactor = 0.93f,
            Offset = new(1),
        };
        AddEntity(_dreamFilter);

        if (Graphics.Debug) {
            _dreamFilter.BlurFactor = 0;
            Graphics.ShaderCache.SetGlobal<IAmbientLightConstantMember, float>(1);
        }

        var coolTexture = Graphics.TextureCache.GetTexture("Assets/173texture.jpg");
        _logoMat = Graphics.MaterialCache.GetMaterial(modelShader, [video.Texture], [gfx.PointSampler]);

        _otherMat = Graphics.MaterialCache.GetMaterial(modelShader, [coolTexture], [gfx.PointSampler]);

        _renderMat = Graphics.MaterialCache.GetMaterial(modelShader, [_dreamFilter.SceneTexture], [gfx.PointSampler]);

        var billboard = Billboard.Create(Graphics, _dreamFilter.SceneTexture);
        billboard.WorldTransform = billboard.WorldTransform with {
            Position = new(2, 2, -0.1f),
        };
        AddEntity(billboard);

        _template = Physics.ModelCache.GetModel("Assets/173_2.b3d").CreateDerivative();

        var template = new AssimpAnimatedModelLoader<AnimatedModelShader, AnimatedModelShader.Vertex, GraphicsResources>(Graphics,
                "Assets/087-B/mental.b3d")
            .LoadAnimatedModel(Graphics.GraphicsDevice);
        var anim = template.Animations.Single().Value;

        AddEntities(Enumerable.Range(0, 3).Select(i => new AnimatedModel(template) {
            WorldTransform = new(Vector3.UnitY + Vector3.UnitX * (i + 4), Quaternion.Identity, new(0.3f)),
            Animation = anim,
            Speed = 5,
            Time = i,
            Looping = true,
        }));

        window.KeyDown += HandleKeyDown;
        window.MouseDown += HandleMouseEvent;
        window.MouseUp += HandleMouseEvent;
    }

    public override void Update(float delta) {
        if (!_paused) {
            if (Graphics.Window.MouseDelta != Vector2.Zero) {
                _player.HandleMouse(Graphics.Window.MouseDelta * 0.01f);
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

        _str.Text = $"""
                    FPS: {_game.FPS}
                    
                    Position: {FormatVec(_player.Camera.WorldTransform.Position)}
                    Rotation: ({ToDeg(_player.Yaw):F3}° ({RadToDir(_player.Yaw)}), {ToDeg(_player.Pitch):F3}°)
                    Velocity: {(_player.Noclip
                        ? $"{_player.Velocity.Length():F3}"
                        : $"({_player.Velocity.XZ().Length():F3}, {_player.Velocity.Y:F3})")}
                    
                    Stamina: {_player.Stamina:F3}
                    BlinkTimer: {_player.BlinkTimer:F3}
                    Floor: {BHelpers.GetFloor(_player.Camera.WorldTransform.Position)}
                    
                    Active Audio Sources: {Source.ActiveSources}
                    """;

        float ToDeg(float rad) => (rad / (2 * MathF.PI) * 360 + 360) % 360;
        string RadToDir(float rad) => ToDeg(rad + 0.25f * MathF.PI) switch {
            < 90 => "South",
            < 180 => "East",
            < 270 => "North",
            _ => "West",
        };
        string FormatVec(Vector3 vec) => $"({vec.X:F3}, {vec.Y:F3}, {vec.Z:F3})";

        base.Update(delta);
    }

    public override void Render(IRenderTarget target, float interp) {
        // TODO: There should be a better hook for this which should also allow access to the base Render method.
        _dreamFilter.RenderScene(target, interp);
    }

    public override void OnLeave() {
        // TODO: This sucks! Might as well eliminate the entire method.
        Graphics.Window.KeyDown -= HandleKeyDown;
        Graphics.Window.MouseDown -= HandleMouseEvent;
        Graphics.Window.MouseUp -= HandleMouseEvent;
    }

    private static string? _serialized;

    private DynamicPhysicsModel? _last173;
    private IReadOnlyList<Model>? _invisCollModels;

    private void HandleKeyDown(KeyEvent e) {
        switch (e.Key) {
            case Key.Space: {
                var entity = (_template with { Meshes = _template.Meshes.Select(x
                        => new MeshMaterial<VPositionTexture>((ICBMesh<VPositionTexture>)x.Mesh, Random.Shared.Next(3) switch {
                            0 => _renderMat,
                            1 => _otherMat,
                            2 => _logoMat,
                        })).Cast<IMeshMaterial>().ToArray() })
                    .InstantiatePhysicsDynamic(1);
                entity.CenterOfMassWorldTransform = _player.Camera.WorldTransform;
                entity.Body.Velocity = new(10 * Vector3.Transform(new(0, 0, 1), _player.Camera.WorldTransform.Rotation));
                AddEntity(entity);
                _last173 = entity;
                break;
            }
            case Key.Number1:
                if (_last173 == null) {
                    break;
                }
                _last173.CenterOfMassWorldTransform = _last173.CenterOfMassWorldTransform with {
                    Scale = _last173.WorldTransform.Scale * 1.1f,
                };
                break;
            case Key.Number2:
                if (_last173 == null) {
                    break;
                }
                _last173.WorldTransform = _last173.WorldTransform with {
                    Position = _last173.WorldTransform.Position + Vector3.UnitY * 20,
                };
                break;
            case Key.KeypadPlus:
                var dic = new Dictionary<ICBShape, ModelTemplate>();
                AddEntities(GetEntitiesOfType<PhysicsModel>()
                    .Select(x => (Model: x, Template: CreateTemplate(x.Collidable.Shape,
                        x.Template.OffsetFromCenter * x.WorldTransform.Scale)))
                    .Where(x => x.Item2 != null)
                    .Select(x => new DebugPhysicsModelFollower(x.Template!, x.Model)));

                AddEntities(_invisCollModels = GetEntitiesOfType<RoomInstance>()
                    .Where(x => x.InvisibleCollision != null)
                    // It'd be really weird if these weren't a mesh.
                    .Select(x => new Model(CreateTemplate(x.InvisibleCollision!.Shape, Vector3.Zero)!) {
                        WorldTransform = x.Transform,
                    }).ToArray());

                ModelTemplate? CreateTemplate(ICBShape shape, Vector3 offset) {
                    if (!dic.TryGetValue(shape, out var template)) {
                        var mesh = shape switch {
                            ICBShape<Mesh> m => m.CreateDebugMesh(Graphics.GraphicsDevice, offset),
                            ICBShape<ConvexHull> ch => ch.CreateDebugMesh(Graphics.GraphicsDevice, offset),
                            _ => null,
                        };
                        if (mesh == null) {
                            return null;
                        }
                        template = new([new MeshMaterial<VPositionNormal>(mesh,
                            Graphics.MaterialCache.GetMaterial<FlatShader, VPositionNormal>())]);
                        dic.Add(shape, template);
                    }
                    return template;
                }
                break;
            case Key.KeypadMinus:
                RemoveEntities(GetEntitiesOfType<DebugPhysicsModelFollower>());
                RemoveEntities(_invisCollModels ?? []);
                break;
            case Key.Escape:
                if (_openMenu != null) {
                    SetOpenMenu(null);
                } else {
                    _game.Scene = new VideoScene(_game, "Assets/Splash_UTG.mp4");
                }
                break;
            case Key.AltLeft: {
                var from = _player.Camera.WorldTransform.Position;
                var to = from + Vector3.Transform(Vector3.UnitZ, _player.Camera.WorldTransform.Rotation) * 5f;
                var line = new DebugLine(Graphics, from, to);
                var cast = Physics.RayCastVisible(from, to);
                line.Color = cast is not null ? new(1, 0, 0) : new(0, 1, 0);
                AddEntity(line);
                if (cast.HasValue) {
                    AddEntity(new DebugLine(Graphics, cast.Value.Pos, cast.Value.Pos + cast.Value.Normal * 0.5f));
                }
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
                // TODO: Serialize items properly.
                RemoveEntities(GetEntitiesOfType<ISerializableEntity>());
                RemoveEntities(GetEntitiesOfType<IItem>());
                break;
            }
            case Key.BackSpace: {
                if (_serialized != null) {
                    AddEntities(SerializationHelper.DeserializeTest(_serialized, Graphics, this));
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
            case Key.N:
                _player.Noclip = !_player.Noclip;
                break;
            case Key.M:
                var pos = _player.Camera.WorldTransform.Position;
                if (_measuringTape.HasValue) {
                    var from = _measuringTape.Value;
                    Log.Information("Measured distance from {From} to {To}: {Distance}", from, pos, Vector3.Distance(from, pos));
                    AddEntity(new DebugLine(Graphics, TimeSpan.FromSeconds(5), from, pos) { Color = new(1, 1, 0) });
                    _measuringTape = null;
                } else {
                    Log.Information("Start measuring from {From}", pos);
                    _measuringTape = pos;
                }
                break;
            case Key.Keypad0:
                GC.Collect();
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
                    elem.Children[i].AddChild(new DebugBorder(Graphics, elem.Children[i].PixelSize, 1f, Color.White) {
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
        _renderMat.Dispose();
        base.DisposeImpl();
    }
}
