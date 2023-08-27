using System.Numerics;
using scpcb.Entities;
using scpcb.Graphics;
using scpcb.Map.RoomProviders;
using scpcb.Utility;

namespace scpcb.Map.Entities; 

public class Light : IMapEntity, IEntityHolder, IRenderable {
    public Billboard Glimmer { get; }
    public Billboard Lensflare { get; }

    public Light(GraphicsResources gfxRes, BillboardManager billboardManager, Transform transform, Vector3 color) {
        Glimmer = billboardManager.Create(gfxRes.TextureCache.GetTexture("Assets/Textures/light1.jpg"), false);
        Glimmer.Transform = transform with {
            Scale = new(0.13f * RMeshRoomProvider.ROOM_SCALE / RMeshRoomProvider.ROOM_SCALE_OLD),
        };

        Lensflare = billboardManager.Create(gfxRes.TextureCache.GetTexture("Assets/Textures/lightsprite.jpg"), true);
        Lensflare.Transform = transform with {
            Scale = new(0.6f * RMeshRoomProvider.ROOM_SCALE / RMeshRoomProvider.ROOM_SCALE_OLD),
            Rotation = Quaternion.CreateFromYawPitchRoll(0f, 0f, Random.Shared.NextSingle() * MathF.PI * 2f),
        };
        Lensflare.Color = color;
    }

    public IEnumerable<IEntity> Entities {
        get {
            yield return Glimmer;
            yield return Lensflare;
        }
    }

    public void Render(RenderTarget target, float interp) {
        Lensflare.Transform = Lensflare.Transform with { Scale = new(RMeshRoomProvider.ROOM_SCALE / RMeshRoomProvider.ROOM_SCALE_OLD
                                                                     * (Random.Shared.NextSingle() * 0.2f + 0.3f)) };
    }
}
