using System.Numerics;
using System.Reflection;
using scpcb.Graphics;
using scpcb.Map.Entities;
using scpcb.Physics;
using scpcb.Utility;

namespace scpcb.Map;

public interface IMapEntityData {
    public IMapEntity Instantiate(GraphicsResources gfxRes, PhysicsResources physics, Transform roomTransform);

    public void AddData(string key, object obj);
}

public class MapEntityData<T> : IMapEntityData where T : IMapEntity {
    private readonly Dictionary<string, object> _values = new();

    private static readonly object _transformMarker = new();

    private readonly Lazy<(ConstructorInfo Ctor, object[] Args, Transform LocalTransform)> _ctorLazy;
    private (ConstructorInfo Ctor, object[] Args, Transform LocalTransform) _ctor => _ctorLazy.Value;

    public MapEntityData(object[] globals) {
        _ctorLazy = new(() => DoThing(globals));
    }

    public IMapEntity Instantiate(GraphicsResources gfxRes, PhysicsResources physics, Transform roomTransform) {
        var (ctor, args, transform) = _ctor;
        object boxedTransform = new Transform(roomTransform.Position + transform.Position,
            roomTransform.Rotation * transform.Rotation,
            roomTransform.Scale * transform.Scale);
        for (var i = 0; i < args.Length; i++) {
            if (args[i] == _transformMarker) {
                args[i] = boxedTransform;
            }
        }
        var ret = (IMapEntity)ctor.Invoke(args);
        for (var i = 0; i < args.Length; i++) {
            if (args[i] == boxedTransform) {
                args[i] = _transformMarker;
            }
        }
        return ret;
    }

    public (ConstructorInfo Ctor, object[] Args, Transform LocalTransform) DoThing(object[] globals) {
        List<object> args = new();
        // TODO: Dealing with transforms like this sucks balls!
        var transform = new Transform((Vector3)_values["position"],
            _values.TryGetValue("rotation", out var rot) ? (Quaternion)rot : Quaternion.Identity,
            _values.TryGetValue("scale", out var scale) ? (Vector3)scale : Vector3.One);
        foreach (var ctor in typeof(T).GetConstructors()) {
            var parameters = ctor.GetParameters();
            args.EnsureCapacity(parameters.Length);
            args.Clear();
            foreach (var prop in parameters) {
                var fromGlobals = globals.FirstOrDefault(x => x.GetType() == prop.ParameterType);
                if (fromGlobals != null) {
                    args.Add(fromGlobals);
                } else if (prop.ParameterType == typeof(Transform)) {
                    args.Add(_transformMarker);
                } else {
                    if (_values.TryGetValue(prop.Name, out var value)) {
                        if (prop.ParameterType == value.GetType()) {
                            args.Add(value);
                        } else {
                            try {
                                args.Add(Convert.ChangeType(value, prop.ParameterType));
                            } catch (InvalidCastException) {
                                break;
                            }
                        }
                    } else if (_values.Values.Count(x => x.GetType() == prop.ParameterType) == 1
                            && parameters.Count(x => x.ParameterType == prop.ParameterType) == 1) {
                            args.Add(_values.Values.First(x => x.GetType() == prop.ParameterType));
                    } else if (prop.HasDefaultValue) {
                        args.Add(prop.DefaultValue!);
                    } else {
                        break;
                    }
                }
            }

            if (args.Count == parameters.Length) {
                return (ctor, args.ToArray(), transform);
            }
        }

        throw new("No fitting ctor found!");
    }

    public void AddData(string key, object obj) {
        _values.Add(key, obj);
    }
}
