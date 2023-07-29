using Veldrid;

namespace scpcb; 

public interface IScene : IDisposable {
    void Update(double delta);
    void Tick();
    void Render(CommandList commandsList, float interp);
}
