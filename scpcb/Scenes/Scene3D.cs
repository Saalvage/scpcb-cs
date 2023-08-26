using scpcb.Entities;
using scpcb.Graphics;

namespace scpcb.Scenes; 

public class Scene3D : BaseScene {
    private readonly GraphicsResources _gfxRes;

    private readonly ModelSorter _sorter = new();

    protected ICamera Camera { get; set; }

    public Scene3D(GraphicsResources gfxRes) {
        _gfxRes = gfxRes;

        OnAddEntity += e => {
            switch (e) {
                case I3DModelProvider p:
                    _sorter.AddRange(p.Models);
                    break;
                case I3DModel m:
                    _sorter.Add(m);
                    break;
            }
        };

        OnRemoveEntity += e => {
            switch (e) {
                case I3DModelProvider p:
                    _sorter.RemoveRange(p.Models);
                    break;
                case I3DModel m:
                    _sorter.Remove(m);
                    break;
            }
        };
    }

    public override void Render(RenderTarget target, float interp) {
        Camera.ApplyTo(_gfxRes.ShaderCache.ActiveShaders.Select(x => x.Constants), interp);

        base.Render(target, interp);

        _sorter.Render(target, Camera.Position, interp);
    }
}
