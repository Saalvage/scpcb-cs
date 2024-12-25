using System.Diagnostics;
using System.Numerics;
using SCPCB.Entities;
using SCPCB.Graphics;
using SCPCB.Map.RoomProviders;
using SCPCB.Scenes;
using SCPCB.Utility;

namespace SCPCB.Map.Entities;

public class Light : IMapEntity, IEntityHolder, IPrerenderable {
    private Billboard _glimmer;
    private Billboard _lensflare;

    private Scene3D _scene;

    public Light(GraphicsResources gfxRes, Transform transform, Vector3 color) {
        _glimmer = Billboard.Create(gfxRes, gfxRes.TextureCache.GetTexture("Assets/Textures/light1.jpg"), true, true, false);
        _glimmer.Transform = transform with {
            Scale = new(0.13f * RMeshRoomProvider.ROOM_SCALE / RMeshRoomProvider.ROOM_SCALE_OLD),
            Rotation = Quaternion.Identity,
        };

        _lensflare = Billboard.Create(gfxRes, gfxRes.TextureCache.GetTexture("Assets/Textures/lightsprite.jpg"), true, true, true);
        _lensflare.Transform = transform with {
            Scale = new(0.6f * RMeshRoomProvider.ROOM_SCALE / RMeshRoomProvider.ROOM_SCALE_OLD),
            Rotation = Quaternion.CreateFromYawPitchRoll(0f, 0f, Random.Shared.NextSingle() * MathF.PI * 2f),
        };
        _lensflare.Color = new(color, 1f);
    }

    public IEnumerable<IEntity> Entities {
        get {
            yield return _glimmer;
            yield return _lensflare;
        }
    }

    public void OnAdd(IScene scene) {
        Debug.Assert(scene is Scene3D);
        _scene = ((Scene3D)scene);
    }

    public void Prerender(float interp) {
        _lensflare.Model.IsVisible = _scene.Physics.RayCastVisible(_scene.Camera.Position, _glimmer.Transform.Position) is null;

        _lensflare.Transform = _lensflare.Transform with {
            Scale = new(RMeshRoomProvider.ROOM_SCALE / RMeshRoomProvider.ROOM_SCALE_OLD
                        * (Random.Shared.NextSingle() * 0.2f + 0.3f)),
        };
    }
}
