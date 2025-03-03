using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Text.Json;
using SCPCB.Audio;
using SCPCB.B.Actions;
using SCPCB.Graphics;
using SCPCB.Graphics.ModelTemplates;
using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders;
using SCPCB.Graphics.Shaders.Vertices;
using SCPCB.Graphics.Text;
using SCPCB.Physics;
using SCPCB.Scenes;
using SCPCB.Utility;

namespace SCPCB.B;

public class MapGenerator {
    private record FloorInfo(string File, decimal Weight, int MinimumFloor = 0, bool Default = false);

    private class RandomFloorPicker {
        private readonly FloorInfo[] _floors;
        private readonly double _maxWeight;
        private readonly string _defaultFloorFile;

        public RandomFloorPicker(FloorInfo[] floors) {
            _floors = floors.CumSumBy(x => x.Weight,
                    (x, sum) => x with { Weight = sum })
                .ToArray();
            _maxWeight = (double)_floors[^1].Weight;
            _defaultFloorFile = floors.Single(x => x.Default).File;
        }

        public string GetRandomFloor(Random rng, int floor) {
            var selector = rng.NextDouble() * _maxWeight;
            var selected = _floors.First(x => selector <= (double)x.Weight);
            return floor >= selected.MinimumFloor ? selected.File : _defaultFloorFile;
        }
    }

    private readonly RandomFloorPicker _picker;

    private readonly Scene3D _scene;
    private readonly PhysicsResources _physics;

    private readonly (FixedFloorActionInfoAttribute Attribute, TypeInfo ActType)[] _fixedActs;
    private readonly (RandomFloorActionInfoAttribute Attribute, TypeInfo ActType)[] _randomActs;

    public MapGenerator(Scene3D scene) {
        _scene = scene;
        _physics = scene.Physics;
        var acts = Assembly.GetExecutingAssembly().DefinedTypes
            .SelectMany(actType => actType.GetCustomAttributes<ActInfoAttribute>().Select(attr => (actType, attr)))
            .GroupBy(x => x.attr.GetType())
            .ToDictionary(x => x.Key, x => x.AsEnumerable());
        // This could also be refactored to support arbitrary attribute functionality via a static method on the attribute type.
        // However, this also appears unneeded.
        _fixedActs = acts.GetValueOrDefault(typeof(FixedFloorActionInfoAttribute))?
            .Select(x => ((FixedFloorActionInfoAttribute)x.attr, x.actType)).ToArray() ?? [];
        _randomActs = acts.GetValueOrDefault(typeof(RandomFloorActionInfoAttribute))?
            .Select(x => ((RandomFloorActionInfoAttribute)x.attr, x.actType)).ToArray() ?? [];

        using var floorsFile = File.OpenRead("Assets/087-B/Floors/floors.json");
        _picker = new(JsonSerializer.Deserialize<FloorInfo[]>(floorsFile)!);
    }

    // We're just straight up adding the map parts to the scene.
    // I currently don't really see a use case for an intermediary data structure.
    // When such a need arises it should be easy enough to refactor.
    public void InstantiateNewMap(int floorCount, int? seed = null) {
        Random rng = seed.HasValue ? new(seed.Value) : new();

        var acts = new IFloorAction?[floorCount];
        foreach (var (attr, actType) in _fixedActs) {
            if (rng.NextDouble() > attr.Probability) {
                continue;
            }

            // There are a few actions that are defined in the same floor range,
            // this does not respect their override order. But do we care?
            acts[rng.Next(attr.MinFloor, attr.MaxFloor + 1)] = (IFloorAction)Activator.CreateInstance(actType);
        }

        SetRandomActsInRange(9, 24, 68);
        SetRandomActsInRange(61, 74, 199);

        void SetRandomActsInRange(int count, int minFloor, int maxFloor) {
            for (var i = 0; i < count; i++) {
                int placeAt;
                do {
                    placeAt = rng.Next(minFloor, maxFloor + 1);
                } while (acts[placeAt] != null);

                var actsInRange = _randomActs
                    .Where(x => placeAt >= x.Attribute.MinFloor && placeAt <= x.Attribute.MaxFloor)
                    .CumSumBy(x => x.Attribute.Weight).ToArray();

                // +1 because there is a chance to not select any action.
                var selected = rng.Next(0, actsInRange[^1].CumSum + 1);
                var act = actsInRange.FirstOrDefault(x => selected < x.CumSum).Item.ActType;
                if (act == null) {
                    continue;
                }

                acts[placeAt] = (IFloorAction)Activator.CreateInstance(act);
            }
        }

        // Floor meshes.
        for (var i = 0; i < floorCount; i++) {
            var newMap = _physics.ModelCache
                .GetModel($"Assets/087-B/Floors/{acts[i]?.PredeterminedFloor ?? _picker.GetRandomFloor(rng, i)}.x", false)
                .InstantiatePhysicsStatic();
            var y = -i * 2;
            newMap.WorldTransform = i % 2 == 0
                ? new(new(0, y, 0), Quaternion.Identity)
                : new(new(8, y, 7), Quaternion.CreateFromYawPitchRoll(MathF.PI, 0, 0));
            _scene.AddEntity(newMap);
        }

        // Glimpses.
        var no = _scene.Audio.SoundCache.GetSound("Assets/087-B/Sounds/no.wav", Channels.Mono);
        for (var i = 1; i < floorCount; i++) {
            if (acts[i] != null || rng.Next(7) != 0) {
                continue;
            }

            _scene.AddEntity(new Glimpse(_scene, _scene.Graphics.TextureCache.GetTexture($"Assets/087-B/glimpse{1 + rng.Next(2)}.png"), no) {
                WorldTransform = new(new(rng.Next(1, 8), -i * 2 - 1, i % 2 == 0 ? 0.3f : 6.55f), Quaternion.Identity, new(0.3f)),
            });
        }

        var sign = _scene.Graphics.TextureCache.GetTexture("Assets/087-B/sign.jpg");

        var cube = new CBMesh<VPositionTexture>(_scene.Graphics.GraphicsDevice, [
            // Front.
            new(new(-0.5f, -0.5f, 0.5f), new(0, 1)),
            new(new(0.5f, -0.5f, 0.5f), new(1, 1)),
            new(new(-0.5f, 0.5f, 0.5f), new(0, 0)),
            new(new(0.5f, 0.5f, 0.5f), new(1, 0)),
            // Back.
            new(new(0.5f, -0.5f, -0.5f), new(0, 1)),
            new(new(-0.5f, -0.5f, -0.5f), new(1, 1)),
            new(new(0.5f, 0.5f, -0.5f), new(0, 0)),
            new(new(-0.5f, 0.5f, -0.5f), new(1, 0)),
            // Left.
            new(new(-0.5f, -0.5f, -0.5f), new(0, 1)),
            new(new(-0.5f, -0.5f, 0.5f), new(1, 1)),
            new(new(-0.5f, 0.5f, -0.5f), new(0, 0)),
            new(new(-0.5f, 0.5f, 0.5f), new(1, 0)),
            // Right.
            new(new(0.5f, -0.5f, 0.5f), new(0, 1)),
            new(new(0.5f, -0.5f, -0.5f), new(1, 1)),
            new(new(0.5f, 0.5f, 0.5f), new(0, 0)),
            new(new(0.5f, 0.5f, -0.5f), new(1, 0)),
            // Top.
            new(new(-0.5f, 0.5f, 0.5f), new(0, 1)),
            new(new(0.5f, 0.5f, 0.5f), new(1, 1)),
            new(new(-0.5f, 0.5f, -0.5f), new(0, 0)),
            new(new(0.5f, 0.5f, -0.5f), new(1, 0)),
            // Bottom.
            new(new(-0.5f, -0.5f, -0.5f), new(0, 1)),
            new(new(0.5f, -0.5f, -0.5f), new(1, 1)),
            new(new(-0.5f, -0.5f, 0.5f), new(0, 0)),
            new(new(0.5f, -0.5f, 0.5f), new(1, 0)),
        ], [0, 1, 2, 3, 2, 1, 4, 5, 6, 7, 6, 5, 8, 9, 10, 11, 10, 9, 12, 13, 14, 15, 14, 13, 16, 17, 18, 19, 18, 17, 20, 21, 22, 23, 22, 21]);
        var signTemplate = new ModelTemplate([
            new MeshMaterial<VPositionTexture>(cube,
                _scene.Graphics.MaterialCache.GetMaterial<ModelShader, VPositionTexture>(
                    [sign],
                    [_scene.Graphics.ClampAnisoSampler])),
        ]);
        var model = signTemplate.Instantiate();
        model.WorldTransform = model.WorldTransform with { Scale = new(0.25f) };
        _scene.AddEntity(model);

        // Floor markers.
        var font = _scene.Graphics.FontCache.GetFont("Assets/087-B/Pretext.TTF", 128);
        for (var i = 1; i <= floorCount; i++) {
            var str = i > 140
                ? string.Join("", Enumerable.Range(0, 4)
                    .TakeWhile(x => x <= rng.Next(4))
                    .Select(_ => (char)rng.Next(33, 123)))
                : rng.Next(600) switch {
                    0 => "",
                    1 => rng.Next(33, 123).ToString(),
                    2 => "NIL",
                    3 => "?",
                    4 => "NO",
                    5 => "stop",
                    _ => (i + 1).ToString(),
                };

            var y = -i * 2 - 0.6f;
            var pos = i % 2 == 0 ? new Vector3(-0.24f, y, 0.5f) : new Vector3(7.4f + 0.6f + 0.24f, y, 7 - 0.5f);
            var markerCube = signTemplate.Instantiate();
            markerCube.WorldTransform = markerCube.WorldTransform with { Position = pos, Scale = new(0.5f) };
            _scene.AddEntity(markerCube);
            _scene.AddEntity(new TextModel3D(_scene.Graphics, font, str) {
                WorldTransform = new(
                    pos + new Vector3(i % 2 == 0 ? 0.251f : -0.251f, 0, 0),
                    Quaternion.CreateFromYawPitchRoll(MathF.PI * (0.5f + (i % 2 == 0 ? 0 : 1)), 0, 0),
                    new(0.001f)),
                Color = Color.Black,
            });
        }
    }
}
