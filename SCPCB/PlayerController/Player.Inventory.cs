﻿using SCPCB.Entities.Items;

namespace SCPCB.PlayerController;

public partial class Player {
    private IItem?[] _items = new IItem?[10];
    public IReadOnlyList<IItem?> Items => _items;

    public bool PickItem(IItem item) {
        foreach (ref var i in _items.AsSpan()) {
            if (i is null) {
                i = item;
                return true;
            }
        }

        return false;
    }

    public void DropItem(IItem item) {
        foreach (ref var i in _items.AsSpan()) {
            if (i == item) {
                i = null;
                break;
            }
        }
    }
}
