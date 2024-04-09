using System.Diagnostics;
using System.Drawing;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.UserInterface.Primitives;

namespace scpcb.Graphics.UserInterface.Composites;

public enum ProgressHandling {
    /// <summary>
    /// BarCount == MaxBarCount <=> percentage >= 100
    /// </summary>
    Floor,
    /// <summary>
    /// BarCount == 0 <=> percentage <= 0
    /// </summary>
    Ceiling,
    /// <summary>
    /// BarCount == 0 and BarCount == MaxBarCount have equal slices of the [0, 1] range.
    /// </summary>
    Fair,
}

public class LoadingBar : Border {
    public int MaxBarCount { get; }

    private int _barCount;
    public int BarCount {
        get => _barCount;
        set {
            Debug.Assert(_barCount <= MaxBarCount);
            for (var i = 0; i < _internalChildren.Count; i++) {
                _internalChildren[i].IsVisible = i < value;
            }
            _barCount = value;
        }
    }

    /// <param name="percentage">In [0, 1]</param>
    public void SetProgress(float percentage, ProgressHandling strategy) {
        if (strategy == ProgressHandling.Fair) {
            BarCount = (int)MathF.Round(Math.Clamp((MaxBarCount + 1) * percentage - 0.5f, 0f, MaxBarCount));
        } else {
            var val = Math.Clamp(MaxBarCount * percentage, 0f, MaxBarCount);
            val = strategy == ProgressHandling.Floor ? val : MathF.Ceiling(val);
            BarCount = (int)val;
        }
    }

    public LoadingBar(GraphicsResources gfxRes, int maxBarCount, ICBTexture texture)
        : base(gfxRes, new(10 * maxBarCount + 4, 20), 1, Color.White) {
        _barCount = MaxBarCount = maxBarCount;
        _internalChildren.Clear();
        _internalChildren.AddRange(Enumerable.Range(0, maxBarCount)
            .Select(i => new TextureElement(gfxRes, texture) {
                Position = new(3 + 10 * i, 3),
            }));
    }
}
