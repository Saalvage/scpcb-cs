using System.Numerics;
using scpcb.Entities.Items;

namespace scpcb.Graphics.UserInterface;

public class Inventory : UIElement {
    private readonly GraphicsResources _gfxRes;
    private readonly UIManager _ui;

    private IReadOnlyList<IItem?> _currItems = [];

    public Inventory(GraphicsResources gfxRes, UIManager ui, IReadOnlyList<IItem?> items) {
        _gfxRes = gfxRes;
        _ui = ui;
        Position = new(0, 35);
        Update(items);
    }

    public void Update(IReadOnlyList<IItem?> items) {
        if (items.Count != _currItems.Count) {
            RecomputeChildren(items);
        } else {
            foreach (var (i, newItem, prevItem) in Enumerable.Range(0, items.Count).Zip(items, _currItems)) {
                if (newItem != prevItem) {
                    Children[i].ClearChildren();
                    if (newItem != null) {
                        Children[i].AddChild(new TextureElement(_gfxRes, newItem.InventoryIcon) {
                            PixelSize = new(64),
                            Z = 10,
                            Alignment = Alignment.Center,
                        });
                    }
                }
            }
        }

        // "Defensive" copy (the array is modified within the player).
        _currItems = [..items];
    }

    private void RecomputeChildren(IReadOnlyList<IItem?> items) {
        ClearChildren();

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
            var border = new TexturedBorder(_gfxRes, _ui, xOffset, xOffset, (_gfxRes.Window.Height / 2f + pos.Y) % 256) {
                PixelSize = new(SIZE),
                Position = pos,
                Alignment = Alignment.Center,
            };
            if (item != null) {
                border.AddChild(new TextureElement(_gfxRes, item.InventoryIcon));
            }
            AddChild(border);
        }
    }

    // We take the row:column ratio that matches the vanilla one (2:5) the closest.
    private static int DetermineRowCount(int itemCount)
        => Enumerable.Range(1, itemCount / 4)
            .Where(rows => itemCount % rows == 0)
            .MinBy(rows => MathF.Abs((rows / (itemCount / (float)rows)) - (2f / 5)));
}
