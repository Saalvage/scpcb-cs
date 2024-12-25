using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;
using System.Text.Json;
using SCPCB.Utility;

namespace SCPCB.Serialization;

public class SerializableDataTypeResolver : DefaultJsonTypeInfoResolver {
    public static SerializableDataTypeResolver Instance { get; } = new();

    private readonly JsonPolymorphismOptions _polyOpt;

    private SerializableDataTypeResolver() {
        _polyOpt = new() {
            UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
        };
        _polyOpt.DerivedTypes.AddRange(Helpers.GetAllLoadedTypes()
            .Where(x => x.IsSubclassOf(typeof(SerializableData)) && !x.IsAbstract)
            .Select(x => new JsonDerivedType(x, x.FullName ?? x.Name))
        );
    }

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options) {
        var jsonTypeInfo = base.GetTypeInfo(type, options);
        if (type == typeof(SerializableData)) {
            jsonTypeInfo.PolymorphismOptions = _polyOpt;
        }

        return jsonTypeInfo;
    }
}
