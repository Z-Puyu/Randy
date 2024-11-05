using System;
using System.Linq;
using Godot;
using RandyPlugin.Generic;

namespace RandyPlugin;

/// <summary>
/// Encapsulates a selector that randomly chooses an object from a collection
/// based on weighted probabilities. The selector permits duplicates.
/// </summary>
[GlobalClass]
public partial class Selector : Selector<object> {
    /// <summary>
    /// Whether the selector should only accept items of a specific type.
    /// </summary>
    [Export] private bool IsTyped { get; set; } = true;
    /// <summary>
    /// The initial collection of items to choose from.
    /// </summary>
    [Export] private SelectableItem[] ItemData { get; set; } = [];
    /// <summary>
    /// The type of the first object added to the selector.
    /// This property is reset when the selector becomes empty.
    /// </summary>
    private Type? DataType { get; set; }

    public override void _Ready() {
        foreach (SelectableItem item in this.ItemData.Where(data => data.Item is not null)) {
            this.Add(item.Item!, item.Weight);
        }
    }

    /// <summary>
    /// Adds an item to the selector with the given weight.
    /// </summary>
    /// <param name="item">The item to be added.</param>
    /// <param name="weight">The weight of the item.</param>
    /// <exception cref="ArgumentException"><paramref name="item"/> is not of the correct type.</exception>
    public override void Add(object item, int weight) {
        if (this.IsTyped) {
            Type type = item.GetType();
            this.DataType ??= type;
            if (type != this.DataType && !type.IsSubclassOf(this.DataType)) {
                throw new ArgumentException(
                    $"Cannot add an item of type {type} to a selector for {this.DataType} objects."
                );
            }
        }
        base.Add(item, weight);
    }

    /// <summary>
    /// Removes a specified number of copies of an item from the selector.
    /// </summary>
    /// <param name="item">The item to be removed.</param>
    /// <param name="k">The number of copies to remove.</param>
    /// <returns>True if the item was removed, false otherwise.</returns>
    /// <exception cref="IndexOutOfRangeException">There are fewer than
    /// <paramref name="k"/> copies of the target item.</exception>
    public override bool Remove(ref object item, int k = 0) {
        bool isSuccessful = base.Remove(ref item, k);
        if (this.IsEmpty) {
            this.DataType = null;
        }
        return isSuccessful;
    }

    /// <summary>
    /// Clears the selector and resets the data type.
    /// </summary>
    public override void Clear() {
        base.Clear();
        this.DataType = null;
    }
}
