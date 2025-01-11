using SCPCB.Graphics.Shaders.ConstantMembers;

namespace SCPCB.Graphics.Shaders.Utility;

public interface IConstantProvider {
    public void ApplyTo(ReadOnlySpan<IConstantHolder?> holders, float interp);
}

public interface IConstantProvider<T, TVal> : IConstantProvider where T : IConstantMember<T, TVal> where TVal : unmanaged, IEquatable<TVal> {
    protected TVal GetValue(float interp);

    void IConstantProvider.ApplyTo(ReadOnlySpan<IConstantHolder?> holders, float interp) => ApplyToInternal(holders, interp);

    void ApplyToInternal(ReadOnlySpan<IConstantHolder?> holders, float interp) {
        var val = GetValue(interp);
        foreach (var holder in holders) {
            holder?.SetValue<T, TVal>(val);
        }
    }
}
