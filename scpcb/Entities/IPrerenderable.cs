namespace scpcb.Entities; 

// TODO: This feels.. off. Instead feature priorities for updatables?
public interface IPrerenderable : IEntity {
    void Prerender(float interp);
}
