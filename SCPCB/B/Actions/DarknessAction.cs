using BepuPhysics.Collidables;
using SCPCB.Audio;
using SCPCB.Entities;
using SCPCB.Graphics;
using SCPCB.Graphics.ModelTemplates;
using SCPCB.Graphics.Shaders;
using SCPCB.Graphics.Shaders.Vertices;
using SCPCB.Graphics.Shapes;
using SCPCB.Physics;
using SCPCB.Physics.Primitives;
using SCPCB.PlayerController;
using SCPCB.Scenes;
using System.Numerics;

namespace SCPCB.B.Actions;

[FixedFloorActionInfo(1, 2)]
public class DarknessAction : FloorActionBase, ITickable {
    private readonly IScene _scene;
    private readonly Player _player;

    private bool _activated = false;
    private bool _done = false;

    private int _timer;

    public DarknessAction(IScene scene) {
        _scene = scene;
        _player = scene.GetEntitiesOfType<Player>().Single();
    }

    public void Tick() {
        Console.WriteLine(IsActive + " " + Floor);
        if (!_activated && IsActive && Vector3.DistanceSquared(_player.Camera.WorldTransform.Position,
                BHelpers.GetFloorCenter(Floor)) < 1) {
            _activated = true;
            var wallTemplate = new PhysicsModelTemplate([
                new MeshMaterial<VPositionTexture>(_scene.Graphics.ShapeCache.GetMesh<Cube>(),
                    _scene.Graphics.MaterialCache.GetMaterial<ModelShader, VPositionTexture>(
                        [_scene.Graphics.TextureCache.GetTexture("Assets/087-B/Floors/brickwall.jpg")],
                        [_scene.Graphics.ClampAnisoSampler])),
            ], new CBShape<Box>(_scene.GetEntitiesOfType<PhysicsResources>().Single(),
                new(1f, 1f, 1f)), Vector3.Zero);
            var wall = wallTemplate.InstantiatePhysicsStatic();
            var pos = BHelpers.GetFloorStart(Floor) + (Floor % 2 == 0 ? -1 : 1) * new Vector3(0.5f, 0, 0);
            wall.WorldTransform = new(pos, Quaternion.Identity, new(1, 2, 1));
            _scene.AddEntity(wall);

            var wall2 = wallTemplate.InstantiatePhysicsStatic();
            var pos2 = BHelpers.GetFloorEnd(Floor) + (Floor % 2 == 0 ? 1 : -1) * new Vector3(0.5f, 0, 0);
            wall2.WorldTransform = new(pos2, Quaternion.Identity, new(1, 2, 1));
            _scene.AddEntity(wall2);
            
            _scene.Audio.PlayFireAndForget("Assets/087-B/Sounds/stone.ogg");
        }

        if (_activated && !_done) {
            _timer++;
            // TODO: Ambient light.
            if (_timer > 600) {
                var enemy = new Enemy(_scene) {
                    WorldTransform = new(BHelpers.GetFloorCenter(Floor)),
                    KillDistance = 0.7f,
                };
                enemy.Tick();
                _scene.AddEntity(enemy);
                _scene.Audio.PlayFireAndForget($"Assets/087-B/Sounds/horror{Random.Shared.Next(1, 4)}.ogg");
                _done = true;
            }
        }
    }
}
