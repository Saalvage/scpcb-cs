namespace scpcb.Entities; 

public interface IUpdatable : IEntity {
    public void Update(float delta);
}
