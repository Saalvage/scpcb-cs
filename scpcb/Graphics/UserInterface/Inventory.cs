using System.Drawing;
using System.Numerics;
using scpcb.Entities.Items;

namespace scpcb.Graphics.UserInterface;

public class Inventory : UIElement {
    private readonly GraphicsResources _gfxRes;

    private IReadOnlyList<IItem?> _currItems = [];

    public Inventory(GraphicsResources gfxRes, IReadOnlyList<IItem?> items) {
        _gfxRes = gfxRes;
        Update(items);
    }

    public void Update(IReadOnlyList<IItem?> items) {
        if (items.Count != _currItems.Count) {
            RecomputeChildren(items);
        } else {
            foreach (var (i, newItem, prevItem) in Enumerable.Range(0, items.Count).Zip(items, _currItems)) {
                if (newItem != prevItem) {
                    Children[i].Children.Clear();
                    if (newItem != null) {
                        Children[i].Children.Add(new TextureElement(_gfxRes, newItem.InventoryIcon));
                    }
                }
            }
        }

        // "Defensive" copy (the array is modified within the player).
        _currItems = [..items];
    }

    private void RecomputeChildren(IReadOnlyList<IItem?> items) {
        Children.Clear();

        const int SIZE = 70;
        const int SPACING = 50;

        var rowCount = DetermineRowCount(items.Count);
        var columnCount = items.Count / rowCount;

        var totalWidth = SIZE * columnCount + SPACING * (columnCount - 1);
        var totalHeight = SIZE * rowCount + SPACING * (rowCount - 1);

        // TODO: Invalid set!?
        PixelSize = new(totalWidth, totalHeight);

        for (var i = 0; i < items.Count; i++) {
            var item = items[i];
            var border = new TextureElement(_gfxRes, _gfxRes.TextureCache.GetTexture(Color.Black)) {
                PixelSize = new(SIZE),
                Position = new Vector2(SIZE / 2f)
                         - PixelSize / 2f
                         + new Vector2(i % columnCount, i / columnCount) * (SIZE + SPACING),
                Alignment = Alignment.Center,
            };
            if (item != null) {
                border.Children.Add(new TextureElement(_gfxRes, item.InventoryIcon));
            }
            Children.Add(border);
        }
    }

    // We take the row:column ratio that matches the vanilla one (2:5) the closest.
    private static int DetermineRowCount(int itemCount)
        => Enumerable.Range(1, itemCount / 4)
            .Where(rows => itemCount % rows == 0)
            .MinBy(rows => MathF.Abs((rows / (itemCount / (float)rows)) - (2f / 5)));
}
