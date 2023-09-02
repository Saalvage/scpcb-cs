using System.Diagnostics;
using System.Numerics;
using scpcb.Entities;
using scpcb.Graphics;
using scpcb.Map.RoomProviders;
using scpcb.Scenes;
using scpcb.Utility;

namespace scpcb.Map.Entities;

public class Light : IMapEntity, IEntityHolder, IPrerenderable {
    private Billboard _glimmer;
    private Billboard _lensflare;

    private Scene3D _scene;

    public Light(GraphicsResources gfxRes, BillboardManager billboardManager, Transform transform, Vector3 color) {
        _glimmer = billboardManager.Create(gfxRes.TextureCache.GetTexture("Assets/Textures/light1.jpg"), false);
        _glimmer.Transform = transform with {
            Scale = new(0.13f * RMeshRoomProvider.ROOM_SCALE / RMeshRoomProvider.ROOM_SCALE_OLD),
            Rotation = Quaternion.Identity,
        };

        _lensflare = billboardManager.Create(gfxRes.TextureCache.GetTexture("Assets/Textures/lightsprite.jpg"), true);
        _lensflare.Transform = transform with {
            Scale = new(0.6f * RMeshRoomProvider.ROOM_SCALE / RMeshRoomProvider.ROOM_SCALE_OLD),
            Rotation = Quaternion.CreateFromYawPitchRoll(0f, 0f, Random.Shared.NextSingle() * MathF.PI * 2f),
        };
        _lensflare.Color = color;
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
        _lensflare.Model.IsVisible = !_scene.Physics.RayCastVisible(_scene.Camera.Position, _glimmer.Transform.Position);

        _lensflare.Transform = _lensflare.Transform with {
            Scale = new(RMeshRoomProvider.ROOM_SCALE / RMeshRoomProvider.ROOM_SCALE_OLD
                        * (Random.Shared.NextSingle() * 0.2f + 0.3f)),
        };
    }
}
