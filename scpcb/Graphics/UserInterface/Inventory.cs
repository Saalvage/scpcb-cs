using System.Numerics;
using scpcb.Entities.Items;
using scpcb.Graphics.UserInterface.Utility;

namespace scpcb.Graphics.UserInterface;

public class Inventory : UIElement {
    private readonly GraphicsResources _gfxRes;
    private readonly UIManager _ui;

    public Inventory(GraphicsResources gfxRes, UIManager ui, IReadOnlyList<IItem?> items) {
        _gfxRes = gfxRes;
        _ui = ui;
        Position = new(0, 35);
        Update(items);
    }

    public void Update(IReadOnlyList<IItem?> items) {
        if (items.Count != _internalChildren.Count) {
            RecomputeChildren(items);
        } else {
            foreach (var (i, item) in Enumerable.Range(0, items.Count).Zip(items)) {
                ((InventorySlot)_internalChildren[i]).Item = item;
            }
        }
    }

    private void RecomputeChildren(IReadOnlyList<IItem?> items) {
        _internalChildren.Clear();

        const int SIZE = 70;
        const int SPACING = 35;

        var rowCount = DetermineRowCount(items.Count);
        var columnCount = items.Count / rowCount;

        var totalWidth = SIZE * columnCount + SPACING * (columnCount - 1);
        var totalHeight = SIZE * rowCount + SIZE * (rowCount - 1);

        // TODO: Invalid set!?
        PixelSize = new(totalWidth, totalHeight);

        for (var i = 0; i < items.Count; i++) {
            var item = items[i];
            var pos = new Vector2(SIZE / 2f)
                           - PixelSize / 2f
                           + new Vector2(i % columnCount * (SIZE + SPACING), i / columnCount * SIZE * 2);
            var xOffset = (_gfxRes.Window.Width / 2f + pos.X - SIZE / 2f) % 64;
            _internalChildren.Add(new InventorySlot(_gfxRes, _ui, SIZE, xOffset, xOffset, (_gfxRes.Window.Height / 2f + pos.Y) % 256) {
                Position = pos,
                Alignment = Alignment.Center,
                Item = item,
            });
        }
    }

    // We take the row:column ratio that matches the vanilla one (2:5) the closest.
    private static int DetermineRowCount(int itemCount)
        => Enumerable.Range(1, itemCount / 4)
            .Where(rows => itemCount % rows == 0)
            .MinBy(rows => MathF.Abs((rows / (itemCount / (float)rows)) - (2f / 5)));
}
