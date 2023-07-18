namespace scpcb.Entities; 

public interface IUpdatable {
    public void Update(double delta);
    public void Tick();
    public void Render(double interpolation);
}
