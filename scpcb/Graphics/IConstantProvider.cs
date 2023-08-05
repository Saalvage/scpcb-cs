using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Graphics.Utility;

namespace scpcb.Graphics;

public interface IConstantProvider {
    public void ApplyTo(IEnumerable<IConstantHolder?> holders, float interp);
}

public interface IConstantProvider<T, TVal> : IConstantProvider where T : IConstantMember<T, TVal> where TVal : unmanaged {
    protected TVal GetValue(float interp);

    void IConstantProvider.ApplyTo(IEnumerable<IConstantHolder?> holders, float interp) {
        var val = GetValue(interp);
        foreach (var holder in holders) {
            holder?.SetValue<T, TVal>(val);
        }
    }
}
