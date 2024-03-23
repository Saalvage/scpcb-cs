using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Utility;
using Veldrid;

namespace scpcb.Graphics.Shaders.Utility;

public interface IConstantHolder : IDisposable {
    bool HasConstant<T>() where T : IConstantMember<T>;
    void SetValue<T, TVal>(TVal val) where T : IConstantMember<T, TVal> where TVal : unmanaged;
    void UpdateAndSetBuffers(CommandList commands, uint index);
}

public interface IConstantHolder<TVertConstants, TFragConstants> : IConstantHolder
        where TVertConstants : unmanaged where TFragConstants : unmanaged { }

public class ConstantHolder<TVertConstants, TFragConstants> : Disposable, IConstantHolder<TVertConstants, TFragConstants>
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
            var propertySize = typeof(T).GetProperties().Sum(x => Marshal.SizeOf(x.PropertyType));
            if (propertySize != 0 && propertySize != sizeof(T)) { // TODO: Deal with 0
                throw new InvalidOperationException("Size of struct does not equal sum of properties");
            }
            return gfx.ResourceFactory.CreateBuffer(new(RoundTo32Bytes(propertySize), BufferUsage.UniformBuffer));

            static uint RoundTo32Bytes(int num) {
                uint run = 32;
                while (run < num) {
                    run += 32;
                }
                return run;
            }
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
        // See Span's SequenceEqual.
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        static unsafe void UpdateBuffer<T>(CommandList commands, DeviceBuffer? buffer, ref T curr, ref T prev, bool force) where T : unmanaged {
            if (buffer == null) {
                return;
            }

            if (force) {
                commands.UpdateBuffer(buffer, 0, ref curr);
                prev = curr;
                return;
            }

            fixed (T* currPtr = &curr) {
                fixed (T* prevPtr = &prev) {
                    var currBytes = (byte*)currPtr;
                    var prevBytes = (byte*)prevPtr;

                    var offset = 0;
                    for (; offset < sizeof(T); offset++) {
                        if (currBytes[offset] != prevBytes[offset]) {
                            break;
                        }
                    }

                    var differentUntil = sizeof(T) - 1;
                    for (; differentUntil > offset; differentUntil--) {
                        if (currBytes[differentUntil] != prevBytes[differentUntil]) {
                            break;
                        }
                    }

                    if (offset != sizeof(T)) {
                        var dataToUpdate = new Span<byte>(currBytes + offset, differentUntil - offset + 1);
                        commands.UpdateBuffer(buffer, (uint)offset, dataToUpdate);
                        prev = curr;
                    }
                }
            }
        }
    }

    public bool HasConstant<T>() where T : IConstantMember<T>
        => _vertexBoxed is T || _fragmentBoxed is T;

    public void SetValue<T, TVal>(TVal val) where T : IConstantMember<T, TVal> where TVal : unmanaged {
        if (_vertexBoxed is T tVert) {
            tVert.Value = val;
            // TODO: We could be checking here if Value == val, might make sense for certain access patterns, needs benchmarking.
            _isDirty = true;
        }
        if (_fragmentBoxed is T tFrag) {
            tFrag.Value = val;
            _isDirty = true;
        }
    }

    protected override void DisposeImpl() {
        _vertexBuffer?.Dispose();
        _fragmentBuffer?.Dispose();
        _set.Dispose();
    }
}
