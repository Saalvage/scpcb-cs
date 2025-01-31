using System.Runtime.CompilerServices;
using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Utility;
using Veldrid;

namespace SCPCB.Graphics.Shaders.Utility;

public interface IConstantHolder : IDisposable {
    bool HasConstant<T>() where T : IConstantMember<T>;

    void TrySetValue<T, TVal>(TVal val) where T : IConstantMember<T, TVal> where TVal : unmanaged, IEquatable<TVal>;
    void SetValue<T, TVal>(TVal val) where T : IConstantMember<T, TVal> where TVal : unmanaged, IEquatable<TVal> {
        if (!HasConstant<T>()) {
            throw new InvalidOperationException($"Holder does not contain constant {typeof(T)}!");
        }
        TrySetValue<T, TVal>(val);
    }

    void SetArrayValue<T, TVal>(int index, TVal val) where T : IConstantArrayMember<T, TVal>
        where TVal : unmanaged, IEquatable<TVal>;

    void UpdateAndSetBuffers(CommandList commands, uint index);
}

public interface IConstantHolder<TVertConstants, TFragConstants> : IConstantHolder
    where TVertConstants : unmanaged where TFragConstants : unmanaged;

public partial class ConstantHolder<TVertConstants, TFragConstants> : Disposable, IConstantHolder<TVertConstants, TFragConstants>
        where TVertConstants : unmanaged where TFragConstants : unmanaged {
    private TVertConstants _lastVert;
    private readonly object _vertexBoxed = default(TVertConstants);
    private ref TVertConstants _vert => ref Unsafe.Unbox<TVertConstants>(_vertexBoxed);

    private TFragConstants _lastFrag;
    private readonly object _fragmentBoxed = default(TFragConstants);
    private ref TFragConstants _frag => ref Unsafe.Unbox<TFragConstants>(_fragmentBoxed);

    private readonly DeviceBuffer? _vertexBuffer;
    private readonly DeviceBuffer? _fragmentBuffer;
    private readonly ResourceSet _set;

    private bool _hasEverUpdated = false;
    private bool _isDirty = true; // Initially true, because we've never updated!

    public static ResourceLayout? TryCreateLayout(GraphicsDevice gfx, string? vertConstantNames, string? fragConstantNames) {
        var hasVertConsts = typeof(TVertConstants) != typeof(Empty);
        var hasFragConsts = typeof(TFragConstants) != typeof(Empty);
        if (!hasVertConsts && !hasFragConsts) {
            return null;
        }

        var layouts = new List<ResourceLayoutElementDescription>();
        if (hasVertConsts) {
            layouts.Add(new(vertConstantNames, ResourceKind.UniformBuffer, ShaderStages.Vertex));
        }

        if (hasFragConsts) {
            layouts.Add(new(fragConstantNames, ResourceKind.UniformBuffer, ShaderStages.Fragment));
        }

        return gfx.ResourceFactory.CreateResourceLayout(new(layouts.ToArray()));
    }

    public static ConstantHolder<TVertConstants, TFragConstants>? TryCreate(GraphicsDevice gfx, ResourceLayout? layout) {
        var hasVertConsts = typeof(TVertConstants) != typeof(Empty);
        var hasFragConsts = typeof(TFragConstants) != typeof(Empty);
        if (!hasVertConsts && !hasFragConsts) {
            return null;
        }

        return new(gfx, layout);
    }

    public unsafe ConstantHolder(GraphicsDevice gfx, ResourceLayout layout) {
        var hasVertConsts = typeof(TVertConstants) != typeof(Empty);
        var hasFragConsts = typeof(TFragConstants) != typeof(Empty);
        if (!hasVertConsts && !hasFragConsts) {
            throw new InvalidOperationException("Constant holder without constants to hold!");
        }

        var consts = new List<BindableResource>();
        if (hasVertConsts) {
            _vertexBuffer = CreateBuffer<TVertConstants>();
            consts.Add(_vertexBuffer);
        }
        if (hasFragConsts) {
            _fragmentBuffer = CreateBuffer<TFragConstants>();
            consts.Add(_fragmentBuffer);
        }

        _set = gfx.ResourceFactory.CreateResourceSet(new(layout, consts.ToArray()));

        DeviceBuffer CreateBuffer<T>() where T : unmanaged {
            // TODO: We used to assert that the size is equal to all the sizes of all properties,
            // I think that was done as a sanity check to prevent unexpected incompatibilities between CPU and GPU.
            // Might make sense to reimplement, but will have to consider fixed size arrays.
            return gfx.ResourceFactory.CreateBuffer(new(Helpers.RoundUpToMultiple((uint)sizeof(T), 32),
                BufferUsage.UniformBuffer));
        }
    }

    public void UpdateAndSetBuffers(CommandList commands, uint index) {
        if (_isDirty) {
            UpdateBuffer(commands, _vertexBuffer, ref _vert, ref _lastVert, !_hasEverUpdated);
            UpdateBuffer(commands, _fragmentBuffer, ref _frag, ref _lastFrag, !_hasEverUpdated);

            _hasEverUpdated = true;
            _isDirty = false;
        }

        commands.SetGraphicsResourceSet(index, _set);

        // This is a pretty hot path (on avg. 1 invocation per shader per frame) so further optimizations might be warranted.
        // 1. We used to calculate the exact portion of the buffer that needs to be updated. Adjusting the SequenceEquals
        // to return the offset where changes begin yielded negative results, likely due to severe pessimization of the SequenceEquals.
        // It might make sense to have another crack at this, since we should get the information *where* we don't change for "free"
        // (at least at the start). To be noted that either way we'd be trading more CPU time for less GPU time.
        // 2. Making it so constant buffers "know" the best way (their most appropriate vector size) to compare themselves should be
        // possible using static interfaces and would allow for cutting down on the branching, likely at the cost of more indirection.
        static unsafe void UpdateBuffer<T>(CommandList commands, DeviceBuffer? buffer, ref T curr, ref T prev, bool force) where T : unmanaged {
            if (buffer == null) {
                return;
            }

            if (force) {
                commands.UpdateBuffer(buffer, 0, ref curr);
                prev = curr;
                return;
            }

            if (!SequenceEqual(ref Unsafe.As<T, byte>(ref curr), ref Unsafe.As<T, byte>(ref prev), (nuint)sizeof(T))) {
                commands.UpdateBuffer(buffer, 0, ref curr);
                prev = curr;
            }
        }
    }

    public bool HasConstant<T>() where T : IConstantMember<T>
        => _vertexBoxed is T || _fragmentBoxed is T;

    public void TrySetValue<T, TVal>(TVal val) where T : IConstantMember<T, TVal> where TVal : unmanaged, IEquatable<TVal> {
        if (_vertexBoxed is T tVert) {
            if (!tVert.Value.Equals(val)) {
                tVert.Value = val;
                _isDirty = true;
            }
        }
        if (_fragmentBoxed is T tFrag) {
            if (!tFrag.Value.Equals(val)) {
                tFrag.Value = val;
                _isDirty = true;
            }
        }
    }

    public void SetArrayValue<T, TVal>(int index, TVal val) where T : IConstantArrayMember<T, TVal> where TVal : unmanaged, IEquatable<TVal> {
        if (_vertexBoxed is T tVert) {
            if (!tVert.Values[index].Equals(val)) {
                tVert.Values[index] = val;
                _isDirty = true;
            }
        }
        if (_fragmentBoxed is T tFrag) {
            if (!tFrag.Values[index].Equals(val)) {
                tFrag.Values[index] = val;
                _isDirty = true;
            }
        }
    }

    protected override void DisposeImpl() {
        _vertexBuffer?.Dispose();
        _fragmentBuffer?.Dispose();
        _set.Dispose();
    }
}
