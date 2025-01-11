using System.Numerics;
using Assimp;
using SCPCB.Graphics.ModelTemplates;

namespace SCPCB.Graphics.Animation;

public record BoneInfo(int Id, Matrix4x4 Offset);
public record ModelAnimationInfo(IReadOnlyDictionary<string, BoneInfo> Bones, Node RootNode);

public interface IAnimatedModelTemplate {
    ModelAnimationInfo Info { get; }
    IReadOnlyDictionary<string, CBAnimation> Animations { get; }
    IModelTemplate BaseTemplate { get; }
}

public record AnimatedModelTemplate<T>(ModelAnimationInfo Info, IReadOnlyDictionary<string, CBAnimation> Animations, T BaseTemplate)
    : IAnimatedModelTemplate where T : IModelTemplate {
    IModelTemplate IAnimatedModelTemplate.BaseTemplate => BaseTemplate;
}

public sealed record OwningAnimatedModelTemplate(ModelAnimationInfo Info,
    IReadOnlyDictionary<string, CBAnimation> Animations, OwningModelTemplate BaseTemplate)
    : AnimatedModelTemplate<OwningModelTemplate>(Info, Animations, BaseTemplate), IDisposable {
    public void Dispose() {
        BaseTemplate.Dispose();
    }
}

public static class AnimatedModelTemplateExtensions {
    public static AnimatedModel Instantiate(this IAnimatedModelTemplate template) => new(template);
}
