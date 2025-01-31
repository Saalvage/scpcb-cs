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
            // TODO: I don't think we should use TrySet here, if a constant holder does
            // not feature a constant provided by a provider, it should not be added.
            holder?.TrySetValue<T, TVal>(val);
        }
    }
}
